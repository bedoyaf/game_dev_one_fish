using System;
using System.Collections.Generic;
using UnityEngine;

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
    public void RemoveComponent(int x, int z, bool showPlaceholder = false) {
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
                    grid[z + i, x + j].ToggleVisibility(showPlaceholder);
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

        /// <summary>
        /// Do not use for placing normal components!!!
        /// </summary>
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