using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

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

    private bool placeholdersVisible;
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

                var gridTile = new ComponentGridTile(j, i);
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
    /// Call when component should be destroyed.
    /// Removes the component and other components if they are not connected to solid.
    /// </summary>
    /// <param name="componentTile"></param>
    public void OnComponentDeath(ComponentGridTile componentTile) {
        RemoveComponent(componentTile.x, componentTile.z, true);
    }

    /// <summary>
    /// Removes the component at the coordinates and replaces it with placeholders
    /// If placeholder parent is not null, it will instantiate placeholders
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="placeholderParent"></param>
    public void RemoveComponent(int x, int z, bool recursive = false) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        if (gridTile.isPlaceholder) return;

        // If recursive, find all components around the tile to later check if they are connected to solid
        List<ComponentGridTile> surroundingTiles = null;
        if (recursive) {
            surroundingTiles = GetTilesAroundComponent(x, z);
        }

        //x -= gridTile.placementOffset.x;
        //z -= gridTile.placementOffset.y;
        var component = gridTile.component;

        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                if (!shouldInstantiate || !shouldInstantiatePlaceholders) {
                    grid[z + i, x + j].PlacePlaceholder(placeholderPrefab, false);
                }
                else {
                    var placeholder = InstantiateComponent(placeholderPrefab, x + j, z + i);
                    grid[z + i, x + j].PlacePlaceholder(placeholder, true);
                    grid[z + i, x + j].ToggleVisibility(placeholdersVisible);
                }
            }
        }

        SetupBlocking(component, x, z, false);

        // Test if the surrounding components are connected to solid after this one was destroyed.
        if (recursive) {
            foreach (var tile in surroundingTiles) {
                if (tile.isPlaceholder) continue;

                (bool connected, var connectedTiles) = IsComponentConnectedToSolid(tile.x, tile.z);
                if (!connected) {
                    foreach (var tmpTile in connectedTiles) {
                        RemoveComponent(tmpTile.x, tmpTile.z);
                    }
                }
            }
        }

        if (recursive) {
            Debug.Log("asdfasdf");
        }
    }

    /// <summary>
    /// Searches the component grid space and tries to find if there is a connection 
    /// between component at coordinates and any solid component.
    /// Search ends when solid is found or searched the whole "graph component"
    /// </summary>
    /// <returns>Whether the component is connected to solid, and also list of all components it met during search</returns>
    public (bool, HashSet<ComponentGridTile> connectedTiles) IsComponentConnectedToSolid(int x, int z) {
        (x, z) = GetOriginTileCoordinates(x, z);

        // Standard bfs algorithm without going back
        var visitedTiles = new HashSet<ComponentGridTile> {
            GetOriginTile(x, z)
        };
        var fringe = new Queue<ComponentGridTile>(GetTilesAroundComponent(x, z));
        while (fringe.Count > 0) {
            var tile = fringe.Dequeue();
            var origin = GetOriginTile(tile.x, tile.z);

            // Have we been here already?
            if (origin.isPlaceholder || visitedTiles.Contains(origin)) continue;
            visitedTiles.Add(origin);

            if (origin.IsSolid) return (true, visitedTiles);

            var surroundings = GetTilesAroundComponent(tile.x, tile.z);
            foreach (var surrounding in surroundings) {
                fringe.Enqueue(surrounding);
            }
        }

        return (false, visitedTiles);
    }

    /// <summary>
    /// Returns all tiles belonging to component at coordinates
    /// </summary>
    public List<ComponentGridTile> GetAllComponentTiles(int x, int z) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var component = grid[z, x].component;
        if (component.placementRules.Height == 1 && component.placementRules.Width == 1) return new List<ComponentGridTile> { grid[z, x] };
        //x = x - grid[z, x].placementOffset.x; 
        //z = z - grid[z, x].placementOffset.y; 
        List<ComponentGridTile> tiles = new List<ComponentGridTile>();
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                tiles.Add(grid[z + i, x + j]);
            }
        }

        return tiles;
    }


    /// <summary>
    /// Places the component in the grid. Removes all that are in the way!
    /// Do not use for placing placeholders - use <see cref="RemoveComponent(int, int)"/>
    /// Works with bigger components
    /// </summary>
    /// <returns>The created component.</returns>
    public ShipComponentController PlaceComponent(ShipComponentController componentPrefab, int x, int z, bool solid = false) {
        var component = componentPrefab;
        if (shouldInstantiate) {
            var newComponent = InstantiateComponent(componentPrefab, x, z);
            component = newComponent;
        }

        // Remove old things in the area of the 
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                RemoveComponent(x + j, z + i);
            }
        }

        // Place the component
        for (int i = 0; i < component.placementRules.Height; i++) {
            for (int j = 0; j < component.placementRules.Width; j++) {
                grid[z + i, x + j].PlaceComponent(component, shouldInstantiate, j, i);
            }
        }

        if (solid) {
            grid[z, x].SetSolid(true);
        } 

        SetupBlocking(component, x, z, true);
        return component;
    }

    public List<ShipComponentController> GetAllComponents() {
        List<ShipComponentController> components = new();
        foreach (var tile in tiles) {
            if (tile.isPlaceholder || tile.hasOffset) continue;

            components.Add(tile.component);
        }

        return components;
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

    private (int, int) GetOriginTileCoordinates(int x, int z) {
        var gridTile = grid[z, x];
        return (x - gridTile.offsetX, z - gridTile.offsetZ);
    }

    private ComponentGridTile GetOriginTile(int x, int z) {
        (x, z) = GetOriginTileCoordinates(x, z);
        return grid[z, x];
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

    public void SetPlaceholderVisibility(bool placeholdersVisible) {
        this.placeholdersVisible = placeholdersVisible;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (grid[i, j].isPlaceholder)
                    grid[i, j].ToggleVisibility(placeholdersVisible);
            }
        }
    }

    /// <summary>
    /// Returns tiles on the all sides of the component on the coordinates
    /// TODO - not tested at all
    /// </summary>
    public List<ComponentGridTile> GetTilesAroundComponent(int x, int z) {
        var tiles = new List<ComponentGridTile>();
        GetTilesOnBottom(x, z, tiles);
        GetTilesOnTop(x, z, tiles);
        GetTilesOnLeft(x, z, tiles);
        GetTilesOnRight(x, z, tiles);

        return tiles;
    }

    /// <summary>
    /// Returns tiles on the right of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnRight(int x, int z, List<ComponentGridTile> output = null) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (x + gridTile.ComponentWidth >= width) return tiles;

        for (int i = 0; i < gridTile.ComponentHeight; i++) {
            tiles.Add(grid[z + i, x + gridTile.ComponentWidth]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the left of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnLeft(int x, int z, List<ComponentGridTile> output = null) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (x <= 0) return tiles;

        for (int i = 0; i < gridTile.ComponentHeight; i++) {
            tiles.Add(grid[z + i, x - 1]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the bottom of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnBottom(int x, int z, List<ComponentGridTile> output = null) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (z <= 0) return tiles;

        for (int j = 0; j < gridTile.ComponentWidth; j++) {
            tiles.Add(grid[z - 1, x + j]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the top of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnTop(int x, int z, List<ComponentGridTile> output = null) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (z + gridTile.ComponentHeight >= height) return tiles;

        for (int j = 0; j < gridTile.ComponentWidth; j++) {
            tiles.Add(grid[z + gridTile.ComponentHeight, x + j]);
        }
        return tiles;
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