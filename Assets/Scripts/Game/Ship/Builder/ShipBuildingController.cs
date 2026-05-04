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
    [Header("Grid settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private bool InitializeOnStart = true;

    [Header("Components and the grid")]
    [SerializeField] List<ShipComponentController> componentPrefabs;
    [SerializeField] ShipData shipData;
    public ComponentGrid componentGrid;

    [Header("Placeholder settings")]
    [SerializeField] private ShipComponentController placeholderComponent;
    [SerializeField] private bool placeholdersVisible;
    //[SerializeField] private bool enabledPlacementRules;

    [Header("Draggable components")]
    public Transform draggablesParent;
    public float draggableDistance = 2;
    public List<ComponentBuildingDrag> draggableComponents;
    public ComponentBuildingDrag currentlyDragging;

    [SerializeField] private ShipController shipController;

    public BuilderMode builderMode;
    private bool isPlayer => builderMode == BuilderMode.Player;

    [Header("Builder arrows")]
    public GameObject arrowPrefab;
    public Transform arrowParent;
    [SerializeField] private Vector3 arrowOffset;
    private List<GameObject> instantiatedArrows = new();

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
        if (InitializeOnStart) {
            InitializeEditor();
        }
        else {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Expects to be called from the ship.
    /// </summary>
    /// <param name="gridToUse">The grid of the ship</param>
    /// <param name="componentPrefabs">Prefabs of components to add. Has to be prefabs!</param>
    public void InitializeBuilder(ComponentGrid gridToUse, List<ShipComponentController> componentPrefabs) {
        componentGrid = gridToUse;
        componentGrid.ChangePlaceholderSetting(true);
        gameObject.SetActive(true);

        // Make placeholders visible
        if (placeholdersVisible) {
            placeholdersVisible = false;
            TogglePlaceholders();
        }

        InitializeDraggableComponents(componentPrefabs);
        MouseController.Instance.enabled = false;
    }

    /// <summary>
    /// End editing and clean up
    /// </summary>
    public void RemoveBuilderConnection() {
        componentGrid.ChangePlaceholderSetting(false);
        componentGrid = null;
        gameObject.SetActive(false);

        draggablesParent.DestroyAllChildren();
        draggableComponents.Clear();
        MouseController.Instance.enabled = true;
    }

    /// <summary>
    /// Initializes the editor for normal editor scene
    /// </summary>
    private void InitializeEditor() {
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

                    componentGrid.PlaceComponent(componentPrefab, j, i, false, shipData[i, j].IsSolid);
                }
            }
        }

        componentGrid.AssignShipController(shipController);
        componentGrid.ConnectGrid(shipData.componentGrid);

        InitializeDraggableComponents(componentPrefabs);
    }

    private void InitializeDraggableComponents(List<ShipComponentController> componentPrefabs) {
        // Make sure we are working with prefabs
        var temp = new List<ShipComponentController>();
        foreach (var comp in componentPrefabs) {
            temp.Add(comp.ComponentPrefab);
        }
        componentPrefabs = temp;

        // Create components for dragging 
        draggableComponents = new();
        float left = 0;
        for (int i = 0; i < componentPrefabs.Count; i++) {
            var comp = componentPrefabs[i];
            var parent = new GameObject(comp.name + "Draggable");
            parent.transform.SetParent(draggablesParent);

            var mesh = Instantiate(comp.ComponentMesh, parent.transform);
            mesh.transform.DestroyAllChildren();
            var collider = Instantiate(comp.ComponentHitbox, parent.transform);
            Destroy(collider.GetComponent<ShipComponentMeshController>());


            parent.transform.localPosition = new Vector3(left, 0, 0);
            left += comp.placementRules.width;
            left += draggableDistance;

            var draggable = parent.AddComponent<ComponentBuildingDrag>();
            draggable.componentPrefab = componentPrefabs[i];
            draggableComponents.Add(draggable);

            //Destroy(tmp);
        }
    }

    /// <summary>
    /// Places component
    /// </summary>
    public void OnClick(InputAction.CallbackContext context) {
        // On press down start drag
        if (context.started) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100)) {
                // TODOH just temporary - not working anymore
                // Toggle solid on a component
                var comp = hit.collider.gameObject.GetComponentInParent<ShipComponentController>();
                if (comp != null && !isPlayer) {
                    var gridTile = comp.placementRules.connectedTile;
                    var tiles = componentGrid.GetAllComponentTiles(gridTile.x, gridTile.z);
                    foreach (var tile in tiles) {
                        componentGrid[tile.z, tile.x].ToggleSolid();
                        shipData[tile.z, tile.x].ToggleSolid();
                    }

                    return;
                }

                // Start dragging an object
                var draggable = hit.collider.gameObject.GetComponentInParent<ComponentBuildingDrag>();
                if (draggable == null) return;

                currentlyDragging = Instantiate(draggable, draggablesParent);
                currentlyDragging.Setup(transform, draggable);
                if (isPlayer) {
                    currentlyDragging.originalObject.gameObject.SetActive(false);
                }

                // Show valid positions
                // Uses object pooling for arrows
                var valid = componentGrid.GetAllValidPositions(draggable.componentPrefab);
                for (int i = instantiatedArrows.Count; i < valid.Count; i++) {
                    instantiatedArrows.Add(Instantiate(arrowPrefab, arrowParent));
                }
                Vector2Int[] directions = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
                for(int i = 0; i < valid.Count; i++) {
                    // Place the arrow on the correct position
                    var tile = valid[i];
                    var arrow = instantiatedArrows[i];
                    arrow.gameObject.SetActive(true);
                    arrow.transform.localPosition = new Vector3(tile.x, 0, tile.z) + arrowOffset;

                    Vector2Int sum = Vector2Int.zero;
                    // Figure out orientation
                    foreach (var dir in directions) {
                        int x = tile.x + dir.x;
                        int z = tile.z - dir.y;

                        if (componentGrid.ValidCoordinates(x, z) && !componentGrid[z, x].isPlaceholder) {
                            sum += dir;
                        }
                    }

                    if (sum != Vector2Int.zero) {
                        Vector2 arrowDirection = new Vector2(sum.x, sum.y).normalized;
                        float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg + 90;
                        arrow.transform.eulerAngles = Vector3.up * angle;
                    }
                    else {
                        arrow.transform.eulerAngles = Vector3.left * -90;
                    }
                }
            }
        }
        // On release place the dragged component
        else if (context.canceled) {
            if (currentlyDragging == null) return;

            var successful = RaycastAndPlaceComponent(currentlyDragging.componentPrefab);

            // Destroy the object and possibly the original one as well
            if (isPlayer) {
                if (successful)
                    currentlyDragging.originalObject.gameObject.SmartDestroy();
                else
                    currentlyDragging.originalObject.gameObject.SetActive(true);
            }

            for (int i = 0; i < instantiatedArrows.Count; i++) {
                instantiatedArrows[i].gameObject.SetActive(false);
            }
            currentlyDragging.gameObject.SmartDestroy();
        }
    }

    /// <summary>
    /// Removes component
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context) {
        if (!context.started || isPlayer) return;
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
                componentGrid.PlaceComponent(componentPrefab, x, z, false, false);
                //shipData.componentGrid.PlaceComponent(componentPrefab, x, z);
            }
            componentGrid.AssignShipController(shipController);
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

    private void OnDestroy() {
        if (shipData != null) {
            shipData.UpdatePossibleDrops();
        }
    }

    public enum BuilderMode {
        Editor,
        Player,
    }
}