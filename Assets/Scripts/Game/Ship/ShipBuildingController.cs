using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ship editor
/// 
/// </summary>
public class ShipBuildingController : MonoBehaviour
{
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    [SerializeField] private ShipComponentController placeholderComponent;

    public Transform DraggablesParent;
    public int DraggableDistance = 2;

    [SerializeField] List<ShipComponentController> componentPrefabs;
    [SerializeField] int selectedComponent;

    [SerializeField] ShipData shipData;

    [SerializeField] private bool placeholdersVisible;
    private ComponentGrid componentGrid;


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
    }

    /// <summary>
    /// Places component
    /// </summary>
    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return;
        RaycastAndPlaceComponent(componentPrefabs[selectedComponent]);
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

            componentGrid.RemoveComponent(x, z);
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

[Serializable]
public class ComponentGrid {
    public List<ComponentGridTile> components = new();
    /// <summary>
    /// Width of the grid
    /// </summary>
    public int width;

    /// <summary>
    /// Height of the grid
    /// </summary>
    public int height;

    public ShipComponentController placeholderPrefab;


    /// <summary>
    /// Where the components are instantiated. If null, we only store reference to prefabs
    /// </summary>
    public Transform componentParent;

    private bool shouldInstantiate => componentParent != null;
    public bool isEmpty => components.Count == 0;

    public ComponentGrid(int width, int height, ShipComponentController placeholderPrefab, Transform componentParent = null) {
        this.width = width;
        this.height = height;
        this.placeholderPrefab = placeholderPrefab;
        this.componentParent = componentParent;
    }

    public ComponentGridTile this[int z, int x] {
        get {
            int index = z * width + x;
            return components[index];
        }
        set {
            int index = z * width + x;
            components[index] = value;
        }
    }

    private ComponentGrid grid => this;

    /// <summary>
    /// Use only for initializing the grid
    /// </summary>
    public void AddPlaceholder(ShipComponentController component) {
        components.Add(new ComponentGridTile(component, shouldInstantiate, true));
    }

    public bool DoesComponentFit(ShipComponentController component, int x, int z) {
        return x + component.placementRules.Width - 1 < width && z + component.placementRules.Height - 1 < height;
    }

    /// <summary>
    /// If placeholder parent is not null, it will instantiate placeholders
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="placeholderParent"></param>
    public void RemoveComponent(int x, int z) {
        var gridTile = grid[z, x];
        if (gridTile.isPlaceholder) return;

        x -= gridTile.placementOffset.x;
        z -= gridTile.placementOffset.y;
        var component = gridTile.component;

        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                if (!shouldInstantiate) {
                    grid[z + i, x + j].PlacePlaceholder(placeholderPrefab);
                }
                else {
                    var placeholder = GameObject.Instantiate(placeholderPrefab, componentParent);
                    placeholder.transform.localPosition = new Vector3(x + j, 0, z + i);
                    grid[z + i, x + j].PlacePlaceholder(placeholder);
                }
            }
        }
    }

    /// <summary>
    /// Places the component in the grid. Removes all that are in the way.
    /// Do not use for placing prefabs - use <see cref="RemoveComponent(int, int)"/>
    /// Works with bigger components
    /// </summary>
    public void PlaceComponent(ShipComponentController componentPrefab, int x, int z) {
        var component = componentPrefab;
        if (shouldInstantiate) {
            var newComponent = GameObject.Instantiate(componentPrefab, componentParent);
            newComponent.transform.localPosition = new Vector3(x, 0, z);

            component = newComponent;
        }

        // Remove old things
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                RemoveComponent(x + j, z + i);
            }
        }

        // Place the component
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                grid[z + i, x + j].PlaceComponent(component, j, i);
            }
        }
    }

    /// <summary>
    /// Checks whether the component can be placed at the coordinates
    /// </summary>
    /// <returns>True if valid placement, false otherwise</returns>
    public bool IsValidPlacementPosition(ShipComponentController component, int x, int z) {
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                if (grid[z + i, x + j].IsBlocked) {
                    return false;
                }
            }
        }

        return true;
    }



    /// <summary>
    /// Helper class providing info about the grid tiles
    /// </summary>
    [Serializable]
    public class ComponentGridTile {
        /// <summary>
        /// // The component in this tile
        /// </summary>
        public ShipComponentController component;

        /// <summary>
        /// Grid offset to the position of the left bottom corner of the component
        /// </summary>
        public Vector2Int placementOffset;

        /// <summary>
        /// How many components are blocking this tile
        /// </summary>
        [SerializeField]
        private int blocked;

        public bool isPlaceholder; // TODO

        public bool hasOffset => placementOffset != Vector2Int.zero;

        public bool IsBlocked => blocked > 0;

        public bool worksWithPrefabs;

        /// <summary>
        /// Blocking is set to false, and no offset
        /// </summary>
        public ComponentGridTile(ShipComponentController component, bool worksWithPrefabs, bool isPlaceholder = false) {
            this.component = component;
            this.isPlaceholder = isPlaceholder;
            this.worksWithPrefabs = worksWithPrefabs;
        }

        private void DestroyCurrentComponent() {
            if (!worksWithPrefabs) return;

            if (component != null && component.gameObject != null) {
                GameObject.Destroy(component.gameObject);
            }
        }

        public void PlacePlaceholder(ShipComponentController placeholder) {
            DestroyCurrentComponent();
            component = placeholder;
            if (!isPlaceholder) {
                RemoveBlock();
                isPlaceholder = true;
            }
            placementOffset = Vector2Int.zero;
        }

        public void PlaceComponent(ShipComponentController component, int offsetX = 0, int offsetZ = 0) {
            DestroyCurrentComponent();
            this.component = component;
            if (isPlaceholder) {
                AddBlock();
                isPlaceholder = false;
            }
            placementOffset = new Vector2Int(offsetX, offsetZ);
        }

        public void ToggleVisibility(bool toggle) {
            component.GetComponentInChildren<MeshRenderer>().enabled = toggle;
        }

        public void AddBlock() {
            blocked++;
        }

        public void RemoveBlock() {
            blocked--;
        }

    }
}