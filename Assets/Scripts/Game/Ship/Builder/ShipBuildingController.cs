using System;
using System.Collections.Generic;
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

    [SerializeField] private bool placeholdersVisible;
    private ComponentGrid componentGrid;

    public Transform draggablesParent;
    public int draggableDistance = 2;
    public List<ComponentBuildingDrag> draggableComponents;
    public ComponentBuildingDrag currentlyDragging;

    void Start()
    {
        // Initialize width, height and the component grid
        bool useShipData = false;
        if (shipData.componentGrid == null || shipData.componentGrid.isEmpty) {
            shipData.componentGrid = new(width, height, placeholderComponent);
        }
        else {
            height = shipData.componentGrid.height;
            width = shipData.componentGrid.width;
            useShipData = true;
        }
        componentGrid = new(width, height, placeholderComponent, transform);

        // Fill the grid with placeholders
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var placeHolder = Instantiate(placeholderComponent, transform);
                placeHolder.transform.localPosition = new Vector3(j, 0, i);
                componentGrid.AddPlaceholder(placeHolder);
                if (!useShipData) {
                    shipData.componentGrid.AddPlaceholder(placeholderComponent);
                }
            }
        }

        // Place the components from the data
        if (useShipData) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    var componentPrefab = shipData[i, j].component;
                    if (shipData[i, j].isPlaceholder || shipData[i, j].placementOffset != Vector2Int.zero) {
                        continue;
                    }
                    componentGrid.PlaceComponent(componentPrefab, j, i);
                }
            }
        }

        if (placeholdersVisible) {
            placeholdersVisible = false;
            TogglePlaceholders();
        }

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
                var draggable = hit.collider.gameObject.GetComponentInParent<ComponentBuildingDrag>();
                if (draggable == null) return;

                currentlyDragging = Instantiate(draggable, draggablesParent);
                currentlyDragging.transform.position = draggable.transform.position + Vector3.up;
                currentlyDragging.beingDragged = true;
                currentlyDragging.GetComponentInChildren<Collider>().enabled = false;
            }
        }
        // On release place the dragged component
        else if (context.canceled) {
            if (currentlyDragging == null) return;

            RaycastAndPlaceComponent(currentlyDragging.componentPrefab);
            Destroy(currentlyDragging.gameObject);
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
    private void RaycastAndPlaceComponent(ShipComponentController componentPrefab, bool isPlaceholder = false) {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100)) {
            var component = hit.collider.gameObject.GetComponentInParent<ShipComponentController>();
            if (component == null) return;
            var position = component.transform.position;
            position = hit.point - component.transform.position + component.transform.localPosition;

            int x = (int)position.x;
            int z = (int)position.z;

            if (!componentGrid.DoesComponentFit(componentPrefab, x, z)) return;

            componentGrid.RemoveComponent(x, z, placeholdersVisible);
            shipData.componentGrid.RemoveComponent(x, z);
            if (!isPlaceholder) {
                componentGrid.PlaceComponent(componentPrefab, x, z);
                shipData.componentGrid.PlaceComponent(componentPrefab, x, z);
            }
        }
    }

    /// <summary>
    /// Toggles the visibility of placeholders
    /// </summary>
    public void TogglePlaceholders() {
        placeholdersVisible = !placeholdersVisible;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (componentGrid[i, j].isPlaceholder)
                    componentGrid[i, j].ToggleVisibility(placeholdersVisible);
            }
        }
    }
}