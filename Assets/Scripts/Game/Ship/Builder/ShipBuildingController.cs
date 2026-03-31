using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ship editor
/// </summary>
public class ShipBuildingController : MonoBehaviour
{
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private ShipComponentController placeholderComponent;

    [SerializeField] List<ShipComponentController> componentPrefabs;

    [SerializeField] int selectedComponent;

    [SerializeField] ShipData shipData;

    public ComponentGrid componentGrid;

    public Transform draggablesParent;
    public float draggableDistance = 2;
    public List<ComponentBuildingDrag> draggableComponents;
    public ComponentBuildingDrag currentlyDragging;

    [SerializeField] private bool placeholdersVisible;
    //[SerializeField] private bool enabledPlacementRules;

    public BuilderMode builderMode;

    private InputAction clickAction;
    private InputAction rightClickAction;

    private void Awake() {
        // Setup mouse
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    private void OnEnable() {
        clickAction.Enable();
        rightClickAction.Enable();
        clickAction.started += OnClick;
        clickAction.canceled += OnClick;
        rightClickAction.started += OnRightClick;
    }

    private void OnDisable() {
        clickAction.Disable();
        rightClickAction.Disable();
        clickAction.started -= OnClick;
        clickAction.canceled -= OnClick;
        rightClickAction.started -= OnRightClick;
    }

    void Start()
    {
        // Initialize width, height and the component grid
        bool useShipData = false;
        if (shipData.componentGrid == null || shipData.componentGrid.isEmpty) {
            shipData.componentGrid = new ComponentGrid(width, height, placeholderComponent, false);
        }
        else {
            height = shipData.componentGrid.height;
            width = shipData.componentGrid.width;
            useShipData = true;
        }
        componentGrid = new ComponentGrid(width, height, placeholderComponent, true, transform);

        // Fill the grid with placeholders
        //for (int i = 0; i < height; i++) {
        //    for (int j = 0; j < width; j++) {
        //        var placeHolder = Instantiate(placeholderComponent, transform);
        //        placeHolder.transform.localPosition = new Vector3(j, 0, i);
        //        componentGrid.AddPlaceholder(placeHolder);
        //        if (!useShipData) {
        //            shipData.componentGrid.AddPlaceholder(placeholderComponent);
        //        }
        //    }
        //}
        componentGrid.InitializeGrid();
        if (!useShipData) {
            shipData.componentGrid.InitializeGrid();
        }

        // Make placeholders visible
        if (placeholdersVisible) {
            placeholdersVisible = false;
            TogglePlaceholders();
        }

        // Place the components from the data
        if (useShipData) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    var componentPrefab = shipData[i, j].component;
                    if (shipData[i, j].hasOffset || shipData[i, j].isPlaceholder) {
                        continue; 
                    }

                    componentGrid.PlaceComponent(componentPrefab, j, i, shipData[i, j].IsSolid);
                }
            }
        }

        componentGrid.AssignConnectedGrid(shipData.componentGrid);

        // Create components for dragging 
        draggableComponents = new();
        for (int i = 0; i < componentPrefabs.Count; i++) {
            var tmp = Instantiate(componentPrefabs[i], draggablesParent);
            tmp.transform.localPosition = new Vector3(i * draggableDistance, 0, 0);

            var obj = tmp.gameObject;
            var draggable = obj.AddComponent<ComponentBuildingDrag>();
            draggable.componentPrefab = componentPrefabs[i];
            draggableComponents.Add(draggable);

            Destroy(tmp);
        }
    }

    /// <summary>
    /// Places component
    /// </summary>
    public void OnClick(InputAction.CallbackContext context) {
        // On press down start drag
        if (context.started) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100)) {
                // TODO just temporary
                var comp = hit.collider.gameObject.GetComponentInParent<ShipComponentController>();
                if (comp != null) {
                    var gridTile = comp.placementRules.connectedTile;
                    var tiles = componentGrid.GetAllComponentTiles(gridTile.x, gridTile.z);
                    foreach (var tile in tiles) {
                        componentGrid[tile.z, tile.x].ToggleSolid();
                        shipData[tile.z, tile.x].ToggleSolid();
                    }

                    return;
                }

                var draggable = hit.collider.gameObject.GetComponentInParent<ComponentBuildingDrag>();
                if (draggable == null) return;

                currentlyDragging = Instantiate(draggable, draggablesParent);
                currentlyDragging.transform.position = draggable.transform.position + Vector3.up;
                currentlyDragging.beingDragged = true;
                currentlyDragging.GetComponentInChildren<Collider>().enabled = false;
                currentlyDragging.GetComponentInChildren<ShipComponentMeshController>().enabled = false;
                currentlyDragging.originalObject = draggable;
                if (builderMode == BuilderMode.Player) {
                    currentlyDragging.originalObject.gameObject.SetActive(false);
                }
            }
        }
        // On release place the dragged component
        else if (context.canceled) {
            if (currentlyDragging == null) return;

            var successful = RaycastAndPlaceComponent(currentlyDragging.componentPrefab);

            // Destroy the object and possibly the 
            if (builderMode == BuilderMode.Player) {
                if (successful)
                    currentlyDragging.originalObject.gameObject.SmartDestroy();
                else
                    currentlyDragging.originalObject.gameObject.SetActive(true);
            }

            currentlyDragging.gameObject.SmartDestroy();
        }
    }

    /// <summary>
    /// Removes component
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context) {
        if (!context.started) return;
        RaycastAndPlaceComponent(placeholderComponent, true); 
    }

    /// <summary>
    /// Raycasts in the scene and tries to place the given component
    /// </summary>
    private bool RaycastAndPlaceComponent(ShipComponentController componentPrefab, bool isPlaceholder = false) {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100)) {
            var component = hit.collider.gameObject.GetComponentInParent<ShipComponentController>();
            if (component == null) return false;

            // Get position of the click
            var position = hit.point - component.transform.position + component.transform.localPosition;
            int x = (int)position.x;
            int z = (int)position.z;

            // Check if the placement is valid/we are not out of bounds
            //if (enabledPlacementRules) {
            //    if (!componentGrid.IsValidPlacementPosition(componentPrefab, x, z, isPlaceholder)) return;
            //}
            //else {
            //    if (!componentGrid.DoesComponentFit(componentPrefab, x, z)) return;
            //}

            // Check if the placement is valid - if we are in player mode, it additionaly has to connect to other component
            if (!componentGrid.IsValidPlacementPosition(componentPrefab, x, z, isPlaceholder, builderMode == BuilderMode.Player))
                return false;

            // Remove previous component and place the new one
            componentGrid.RemoveComponent(x, z);
            //shipData.componentGrid.RemoveComponent(x, z);
            if (!isPlaceholder) {
                componentGrid.PlaceComponent(componentPrefab, x, z);
                //shipData.componentGrid.PlaceComponent(componentPrefab, x, z);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Toggles the visibility of placeholders
    /// </summary>
    public void TogglePlaceholders() {
        placeholdersVisible = !placeholdersVisible;
        componentGrid.SetPlaceholderVisibility(placeholdersVisible);
    }

    public enum BuilderMode {
        Editor,
        Player,
    }
}