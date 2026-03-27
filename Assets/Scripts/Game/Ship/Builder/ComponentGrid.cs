using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86.Avx;

[Serializable]
public class ComponentGrid {

    /// <summary>
    /// 1D represetation of the component grid. Has to be 1D so that Unity serializes it.
    /// </summary>
    [FormerlySerializedAs("components")]
    public List<ComponentGridTile> tiles = new();

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


    /// <summary>
    /// Whether specifically placeholders should be instantiated.
    /// Is sub-category of <see cref="shouldInstantiate"/>
    /// </summary>
    public bool shouldInstantiatePlaceholders;

    /// <summary>
    /// Whether the components should be instantiated when being placed.
    /// </summary>
    private bool shouldInstantiate => componentParent != null;

    public bool isEmpty => tiles.Count == 0;

    public ComponentGrid(int width, int height, ShipComponentController placeholderPrefab, bool shouldInstantiatePlaceholders, Transform componentParent = null) {
        this.width = width;
        this.height = height;
        this.placeholderPrefab = placeholderPrefab;
        this.componentParent = componentParent;
        this.shouldInstantiatePlaceholders = shouldInstantiatePlaceholders;
    }

    /// <summary>
    /// Allows grid-like access
    /// </summary>
    public ComponentGridTile this[int z, int x] {
        get {
            int index = z * width + x;
            return tiles[index];
        }
        set {
            int index = z * width + x;
            tiles[index] = value;
        }
    }

    private ComponentGrid grid => this;

    /// <summary>
    /// Fills the grid with placeholders.
    /// </summary>
    /// <param name="instantiatePlaceholders">Whether the placeholders should be instantiated or not</param>
    public void InitializeGrid() {
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var placeholder = placeholderPrefab;
                bool instantiate = shouldInstantiate && shouldInstantiatePlaceholders;
                if (instantiate) {
                    placeholder = InstantiateComponent(placeholderPrefab, j, i);
                }

                var gridTile = new ComponentGridTile(i, j);
                gridTile.PlacePlaceholder(placeholder, instantiate);
                tiles.Add(gridTile);
            }
        }
    }

    public void DestroyGrid() {
        if (shouldInstantiate && !isEmpty) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    grid[i, j].DestroyCurrentComponent();
                    //var comp = grid[i, j].component;
                    //if (comp == null || comp.gameObject == null) continue;
                    //comp.gameObject.SmartDestroy();
                }
            }
        }

        tiles.Clear();
    }

    ///// <summary>
    ///// Creates a 1 to 1 copy of this grid.
    ///// If <paramref name="componentParent"/> is filled, will use it as parent and initialize the components.
    ///// Otherwise, it will just store the prefabs.
    ///// TODO - this has to be only on ship data
    ///// </summary>
    //public void CreateCopy(Transform componentParent = null, bool instantiatePlaceholders = false) {
    //    var otherGrid = new ComponentGrid(width, height, placeholderPrefab, componentParent);
    //    otherGrid.InitializeGrid(instantiatePlaceholders);
    //    for (int i = 0; i < height; i++) {
    //        for (int j = 0; j < width; j++) {
    //            var gridTile = grid[i, j];
    //            if (gridTile.placementOffset != Vector2Int.zero || gridTile.isPlaceholder) {
    //                continue;
    //            }

    //            otherGrid.PlaceComponent(gridTile.component, j, i);
    //        }
    //    }
    //}

    /// <summary>
    /// Instantiates the component and places it at correct local! position 
    /// </summary>
    private ShipComponentController InstantiateComponent(ShipComponentController componentPrefab, int x, int z) {
        ShipComponentController component = null;
#if UNITY_EDITOR
        if (Application.isPlaying) {
            component = GameObject.Instantiate(componentPrefab, componentParent);
        }
        else {
            component = PrefabUtility.InstantiatePrefab(componentPrefab, componentParent) as ShipComponentController;
        }
#else
        component = GameObject.Instantiate(componentPrefab, componentParent);
#endif
        component.transform.localPosition = new Vector3(x, 0, z);
        return component;
    }

    ///// <summary>
    ///// Use only for initializing the grid
    ///// </summary>
    //public void AddPlaceholder(ShipComponentController component) {
    //    components.Add(new ComponentGridTile(component, shouldInstantiate, true));
    //}

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
                if (!shouldInstantiate || !shouldInstantiatePlaceholders) {
                    grid[z + i, x + j].PlacePlaceholder(placeholderPrefab, false);
                }
                else {
                    var placeholder = InstantiateComponent(placeholderPrefab, x + j, z + i);
                    grid[z + i, x + j].PlacePlaceholder(placeholder, true);
                    grid[z + i, x + j].ToggleVisibility(showPlaceholder);
                }
            }
        }

        SetupBlocking(component, x, z, false);
    }

    /// <summary>
    /// Places the component in the grid. Removes all that are in the way!
    /// Do not use for placing placeholders - use <see cref="RemoveComponent(int, int)"/>
    /// Works with bigger components
    /// </summary>
    /// <returns>The created component.</returns>
    public ShipComponentController PlaceComponent(ShipComponentController componentPrefab, int x, int z, bool showPlaceholders = false) {
        var component = componentPrefab;
        if (shouldInstantiate) {
            var newComponent = InstantiateComponent(componentPrefab, x, z);

            component = newComponent;
        }

        // Remove old things in the area of the 
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                RemoveComponent(x + j, z + i, showPlaceholders);
            }
        }

        // Place the component
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                grid[z + i, x + j].PlaceComponent(component, shouldInstantiate, j, i);
            }
        }

        SetupBlocking(component, x, z, true);
        return component;
    }

    /// <summary>
    /// Add block to surroundings of the component
    /// x and z are the left bottom
    /// </summary>
    public void SetupBlocking(ShipComponentController component, int x, int z, bool addBlock) {
        if (!component.placementRules.blockSurroundings) return;

        int componentHeight = component.placementRules.Height;
        int componentWidth = component.placementRules.Width;

        // Boundaries (exclusive)
        int top = Mathf.Min(z + componentHeight + component.placementRules.Top, height);
        int bottom = Mathf.Max(z - component.placementRules.Bottom - 1, -1);
        int left = Mathf.Max(x - component.placementRules.Left - 1, -1);
        int right = Mathf.Min(x + componentWidth + component.placementRules.Right, width);


        // Top
        for (int j = 0; j < component.placementRules.Width; j++) {
            for (int i = z + componentHeight; i < top; i++) {
                grid[i, x + j].ChangeBlock(addBlock);
            }
        }

        // Bottom
        for (int j = 0; j < component.placementRules.Width; j++) {
            for (int i = z - 1; i > bottom; i--) {
                grid[i, x + j].ChangeBlock(addBlock);
            }
        }

        // Left
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = x - 1; j > left; j--) {
                grid[z + i, j].ChangeBlock(addBlock);
            }
        }

        // Right
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = x + componentWidth; j < right; j++) {
                grid[z + i, j].ChangeBlock(addBlock);
            }
        }
    }

    /// <summary>
    /// Checks whether the component can be placed at the coordinates
    /// </summary>
    /// <returns>True if valid placement, false otherwise</returns>
    public bool IsValidPlacementPosition(ShipComponentController component, int x, int z, bool isPlaceholder) {
        if (isPlaceholder) return true;
        if (!DoesComponentFit(component, x, z)) return false;

        // Check if any tile is blocked
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                if (grid[z + i, x + j].isBlocked) {
                    return false;
                }
            }
        }

        // Check if it connects to something - TODO

        return true;
    }



    /// <summary>
    /// Helper class providing info about the grid tiles
    /// </summary>
    [Serializable]
    public class ComponentGridTile {
        /// <summary>
        /// The component in this tile
        /// </summary>
        public ShipComponentController component;

        /// <summary>
        /// The x coordinate of the tile
        /// </summary>
        public int x;

        /// <summary>
        /// The z coordinate of the tile
        /// </summary>
        public int z;

        /// <summary>
        /// Grid offset to the position of the left bottom corner of the component
        /// </summary>
        public Vector2Int placementOffset;

        /// <summary>
        /// How many components are blocking this tile
        /// </summary>
        [SerializeField]
        private int blocked;

        public int GetBlock => blocked;

        public bool isPlaceholder; // TODO

        public bool hasOffset => placementOffset != Vector2Int.zero;

        public bool isBlocked => blocked > 0;

        //public bool worksWithPrefabs;

        private bool visible;
        private bool instantiatedComponent;

        /// <summary>
        /// Blocking is set to false, and no offset
        /// </summary>
        public ComponentGridTile(int x, int z) {
            this.x = x;
            this.z = z;
        }

        public void DestroyCurrentComponent() {
            if (!instantiatedComponent) return;

            if (component != null && component.gameObject != null) {
                component.gameObject.SmartDestroy();
            }
        }

        /// <summary>
        /// Do not use for placing normal components!!!
        /// </summary>
        public void PlacePlaceholder(ShipComponentController placeholder, bool isInstantiated) {
            DestroyCurrentComponent();
            component = placeholder;
            if (!isPlaceholder) {
                isPlaceholder = true;
                ChangeBlock(false);
            }
            placementOffset = Vector2Int.zero;

            instantiatedComponent = isInstantiated;
            if (!isInstantiated) return;
            placeholder.placementRules.connectedTile = this;
        }

        /// <summary>
        /// Do not use for placing placeholders!
        /// Stores the component in the grid
        /// </summary>
        public void PlaceComponent(ShipComponentController component, bool isInstantiated, int offsetX = 0, int offsetZ = 0) {
            DestroyCurrentComponent();
            this.component = component;
            if (isPlaceholder) {
                isPlaceholder = false;
                ChangeBlock(true);
            }
            placementOffset = new Vector2Int(offsetX, offsetZ);

            instantiatedComponent = isInstantiated;
            if (!isInstantiated) return;
            component.placementRules.connectedTile = this;
        }

        public void ToggleVisibility(bool toggle) {
            component.GetComponentInChildren<MeshRenderer>().enabled = toggle;
            visible = toggle;
        }

        public void ChangeBlock(bool add) {
            if (add) AddBlock();
            else RemoveBlock();

            PlaceholderColor();
        }
        public void ChangeBlock(int add) {
            blocked += add;

            PlaceholderColor();
        }

        private void PlaceholderColor() {
            if (!visible || !isPlaceholder) return;

            if (isBlocked) {
                component.GetComponentInChildren<MeshRenderer>().material.color = Color.lightCyan;
            }
            else {
                component.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }

        

        public void AddBlock() {
            blocked++;
        }

        public void RemoveBlock() {
            blocked--;
        }
    }
}

public static class GameObjectExtensions {
    public static void SmartDestroy(this GameObject gameObject) {
        if (Application.isPlaying) {
            GameObject.Destroy(gameObject);
        }
        else {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}