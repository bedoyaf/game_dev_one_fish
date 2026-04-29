using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static ShipComponentController;

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

    /// <summary>
    /// Just for debug
    /// </summary>
    public int ID = UnityEngine.Random.Range(-2147483648, 2147483647);

    /// <summary>
    /// This grid will get same place and remove commands as this one
    /// </summary>
    private ComponentGrid connectedGrid;

    private bool everythingSolid;

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

    /// <summary>
    /// Destroys all components and tiles in the grid.
    /// </summary>
    public void DestroyGrid() {
        if (shouldInstantiate && !isEmpty) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    grid[i, j].DestroyCurrentComponent();
                }
            }
        }

        tiles.Clear();
    }


    /// <summary>
    /// Places the component in the grid. Removes all that are in the way!
    /// Do not use for placing placeholders - use <see cref="RemoveComponent(int, int)"/>
    /// Works with bigger components
    /// </summary>
    /// <param name="alreadyInstantiated">Whether the component is already instantiated, so I should not instantiate it again.</param>
    /// <param name="solid">Whether the component should be solid or not</param>
    /// <returns>The created component.</returns>
    public ShipComponentController PlaceComponent(ShipComponentController componentPrefab, int x, int z, bool alreadyInstantiated, bool solid) {
        var component = componentPrefab;
        if (shouldInstantiate && !alreadyInstantiated) {
            var newComponent = InstantiateComponent(componentPrefab, x, z);
            component = newComponent;
        }

        // Remove old things in the area of the 
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = 0; j < component.placementRules.width; j++) {
                RemoveComponent(x + j, z + i);
            }
        }

        // Place the component
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = 0; j < component.placementRules.width; j++) {
                grid[z + i, x + j].PlaceComponent(component, shouldInstantiate, j, i);
                if (solid || everythingSolid || component.placementRules.solid) {
                    grid[z + i, x + j].SetSolid(true);
                }
            }
        }

        SetupBlocking(component, x, z, true);

        if (connectedGrid != null)
            connectedGrid.PlaceComponent(componentPrefab, x, z, alreadyInstantiated, solid);

        return component;
    }

    /// <summary>
    /// Removes the component at the coordinates and replaces it with placeholders
    /// If placeholder parent is not null, it will instantiate placeholders
    /// </summary>
    /// <param name="recursive">If true, it will check if surrounding components are connected to solid and potentially remove them</param>
    public void RemoveComponent(int x, int z, bool recursive = false) {
        if (connectedGrid != null) connectedGrid.RemoveComponent(x, z, recursive);
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

        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = 0; j < component.placementRules.width; j++) {
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
    }

    /// <summary>
    /// Changes between instanciating and not instaciating placeholders.
    /// </summary>
    public void ChangePlaceholderSetting(bool shouldInstantiatePlaceholders) {
        if (this.shouldInstantiatePlaceholders == shouldInstantiatePlaceholders) return;

        this.shouldInstantiatePlaceholders = shouldInstantiatePlaceholders;
        for(int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var tile = grid[i, j];
                if (tile.isPlaceholder) {
                    if (!shouldInstantiate || !shouldInstantiatePlaceholders) {
                        tile.PlacePlaceholder(placeholderPrefab, false);
                    }
                    else {
                        var placeholder = InstantiateComponent(placeholderPrefab, j, i);
                        tile.PlacePlaceholder(placeholder, true);
                        tile.ToggleVisibility(placeholdersVisible);
                    }
                }
            }
        }
    }

    ///// <summary>
    ///// Use only for initializing the grid
    ///// </summary>
    //public void AddPlaceholder(ShipComponentController component) {
    //    components.Add(new ComponentGridTile(component, shouldInstantiate, true));
    //}
    public bool DoesComponentFit(ShipComponentController component, int x, int z) {
        return x + component.placementRules.width - 1 < width && z + component.placementRules.height - 1 < height;
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
        if (component.placementRules.height == 1 && component.placementRules.width == 1) return new List<ComponentGridTile> { grid[z, x] };
        //x = x - grid[z, x].placementOffset.x; 
        //z = z - grid[z, x].placementOffset.y; 
        List<ComponentGridTile> tiles = new List<ComponentGridTile>();
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = 0; j < component.placementRules.width; j++) {
                tiles.Add(grid[z + i, x + j]);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Add block to surroundings of the component
    /// x and z are the left bottom
    /// </summary>
    public void SetupBlocking(ShipComponentController component, int x, int z, bool addBlock) {
        if (!component.placementRules.blockSurroundings) return;

        int componentHeight = component.placementRules.height;
        int componentWidth = component.placementRules.width;

        // Boundaries (exclusive)
        int top = Mathf.Min(z + componentHeight + component.placementRules.top, height);
        int bottom = Mathf.Max(z - component.placementRules.bottom - 1, -1);
        int left = Mathf.Max(x - component.placementRules.left - 1, -1);
        int right = Mathf.Min(x + componentWidth + component.placementRules.right, width);


        // Top
        for (int j = 0; j < component.placementRules.width; j++) {
            for (int i = z + componentHeight; i < top; i++) {
                grid[i, x + j].ChangeBlock(addBlock);
            }
        }

        // Bottom
        for (int j = 0; j < component.placementRules.width; j++) {
            for (int i = z - 1; i > bottom; i--) {
                grid[i, x + j].ChangeBlock(addBlock);
            }
        }

        // Left
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = x - 1; j > left; j--) {
                grid[z + i, j].ChangeBlock(addBlock);
            }
        }

        // Right
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = x + componentWidth; j < right; j++) {
                grid[z + i, j].ChangeBlock(addBlock);
            }
        }
    }

    /// <summary>
    /// Gets all components that are stored in the grid.
    /// Multiple-tile components are still returned only once.
    /// </summary>
    /// <returns></returns>
    public List<ShipComponentController> GetAllComponents() {
        List<ShipComponentController> components = new();
        foreach (var tile in tiles) {
            if (tile.isPlaceholder || tile.hasOffset) continue;

            components.Add(tile.component);
        }

        return components;
    }

    /// <summary>
    /// Gets all unique (meaning different prefabs) components that are stored in the grid.
    /// </summary>
    /// <returns></returns>
    public List<ShipComponentController> GetAllUniqueComponentPrefabs() {
        HashSet<ShipComponentController> components = new();
        foreach (var tile in tiles) {
            if (tile.isPlaceholder || tile.hasOffset) continue;

            components.Add(tile.component.componentPrefab);
        }

        return components.ToList();
    }

    /// <summary>
    /// Gets all components that are stored in the grid, non broken ones.
    /// </summary>
    public List<ShipComponentController> GetAllNonBrokenComponents()
    {
        List<ShipComponentController> components = new();
        foreach (var tile in tiles)
        {
            if (tile.isPlaceholder || tile.hasOffset) continue;

            if (tile.component.broken) continue;
            components.Add(tile.component);
        }

        return components;
    }

    /// <summary>
    /// Returns a list of all components from the grid that contain the specific behaviour
    /// </summary>
    public List<T> GetComponentsOfType<T>(bool includeBroken = true) where T : BehaviourComponentControllerAbstract {
        List<T> result = new List<T>();

        // Debug.Log(GetAllComponents().Count);

        foreach (var comp in GetAllComponents()) {
            var behaviour = comp.GetComponent<T>();

            if (behaviour != null && (!behaviour.GetComponent<ShipComponentController>().broken || includeBroken)) {
                result.Add(behaviour);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a dictionary, whose key is the component data (ID) and whose value is how many
    /// times is unbroken! component present in the grid.
    /// </summary>
    public Dictionary<ShipComponentData, int> GetNonBrokenComponentsCountGroupedByData() {
        Dictionary<ShipComponentData, int> componentCounts = new();
        foreach (var tile in tiles) {
            if (tile.isPlaceholder || tile.hasOffset) continue;

            if (tile.component.broken) continue;
            var data = tile.component.componentData;
            if (componentCounts.ContainsKey(data)) {
                componentCounts[data]++;
            }
            else {
                componentCounts.Add(data, 1);
            }
        }

        return componentCounts;
    }

    /// <summary>
    /// Checks if any component is solid in the grid
    /// </summary>
    public bool ContainsSolid() {
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (grid[i, j].IsSolid) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets every single component solid
    /// </summary>
    public void SetEverythingSolid() {
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                grid[i, j].SetSolid(true);
            }
        }
        everythingSolid = true;
    }

    /// <summary>
    /// Copies this grid into another grid.
    /// Expects newly created grid.
    /// If this grid already has instantiated component, only copies the refences.
    /// </summary>
    /// <param name="outputGrid">The target grid. Will be overridden.</param>
    /// <param name="componentsInstantiated">If current grid has already instantiated components</param>
    public void CopyComponentGrid(ComponentGrid outputGrid, bool componentsInstantiated = false) {
        if (outputGrid.tiles.Count > 0) {
            outputGrid.DestroyGrid();
        }
        outputGrid.InitializeGrid();

        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var gridTile = grid[i, j];
                if (gridTile.hasOffset || gridTile.isPlaceholder) {
                    continue;
                }

                outputGrid.PlaceComponent(gridTile.component, j, i, componentsInstantiated, gridTile.IsSolid || everythingSolid);
            }
        }
    }

    /// <summary>
    /// Returns all tiles on which the component can be placed.
    /// </summary>
    public List<ComponentGridTile> GetAllValidPositions(ShipComponentController component) {
        var tiles = new List<ComponentGridTile>();
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (IsValidPlacementPosition(component, j, i, false, true)) {
                    tiles.Add(grid[i, j]);
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Assigns a grid that will receive the same place and remove component calls.
    /// </summary>
    public void ConnectGrid(ComponentGrid grid) {
        connectedGrid = grid;
    }

    /// <summary>
    /// Cuts the connection to the other grid
    /// </summary>
    public void RemoveConnectedGrid() {
        connectedGrid = null;
    }

    /// <summary>
    /// Gets the coordinates of the left bottom tile of the component at this coordinates.
    /// Useful for multi-tile components, for others it returns the same coordinates.
    /// </summary>
    private (int, int) GetOriginTileCoordinates(int x, int z) {
        var gridTile = grid[z, x];
        return (x - gridTile.offsetX, z - gridTile.offsetZ);
    }

    /// <summary>
    /// Gets the left bottom tile of the component at this coordinates.
    /// Useful for multi-tile components, for others it returns just the tile at the coordinates
    /// </summary>
    private ComponentGridTile GetOriginTile(int x, int z) {
        (x, z) = GetOriginTileCoordinates(x, z);
        return grid[z, x];
    }

    /// <summary>
    /// Checks whether the component can be placed at the coordinates
    /// </summary>
    /// <param name="component">The component I'm trying to place</param>
    /// <param name="x">The x coordinate</param>
    /// <param name="z">The z coordinate</param>
    /// <param name="isPlaceholder">Whether the component I'm placing is a placeholder</param>
    /// <param name="mustConnect">Whether the component must be connected to some other component</param>
    /// <returns>True if valid placement, false otherwise</returns>
    public bool IsValidPlacementPosition(ShipComponentController component, int x, int z, bool isPlaceholder, bool mustConnect) {
        if (isPlaceholder) return true;
        if (!DoesComponentFit(component, x, z)) return false;

        // Check if it connects to something
        if (mustConnect) {
            bool foundNeighbor = false;
            var surroundings = GetTilesAroundComponent(x, z, component);
            foreach (var tile in surroundings) {
                if (!tile.isPlaceholder) {
                    foundNeighbor = true;
                    break;
                }
            }
            if (!foundNeighbor) return false;
        }

        // Check if any tile is blocked
        for (int i = 0; i < component.placementRules.height; i++) {
            for (int j = 0; j < component.placementRules.width; j++) {
                if (grid[z + i, x + j].isBlocked) {
                    return false;
                }
            }
        }

        // If the block is blocking, check if to-be-blocked tiles are empty
        // TODO copied from SetupBlocking
        if (component.placementRules.blockSurroundings) {
            int componentHeight = component.placementRules.height;
            int componentWidth = component.placementRules.width;

            // Boundaries (exclusive)
            int top = Mathf.Min(z + componentHeight + component.placementRules.top, height);
            int bottom = Mathf.Max(z - component.placementRules.bottom - 1, -1);
            int left = Mathf.Max(x - component.placementRules.left - 1, -1);
            int right = Mathf.Min(x + componentWidth + component.placementRules.right, width);

            // Top
            for (int j = 0; j < component.placementRules.width; j++) {
                for (int i = z + componentHeight; i < top; i++) {
                    if (!grid[i, x + j].isPlaceholder) return false;
                }
            }

            // Bottom
            for (int j = 0; j < component.placementRules.width; j++) {
                for (int i = z - 1; i > bottom; i--) {
                    if (!grid[i, x + j].isPlaceholder) return false;
        }
            }

            // Left
            for (int i = 0; i < component.placementRules.height; i++) {
                for (int j = x - 1; j > left; j--) {
                    if (!grid[z + i, j].isPlaceholder) return false;
                }
            }

            // Right
            for (int i = 0; i < component.placementRules.height; i++) {
                for (int j = x + componentWidth; j < right; j++) {
                    if (!grid[z + i, j].isPlaceholder) return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// For editor only. Makes placeholders in/visible
    /// </summary>
    public void SetPlaceholderVisibility(bool placeholdersVisible) {
        this.placeholdersVisible = placeholdersVisible;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (grid[i, j].isPlaceholder)
                    grid[i, j].ToggleVisibility(placeholdersVisible);
            }
        }
    }

    public bool ValidCoordinates(int x, int z) {
        return x >= 0 && z >= 0 && x < width && z < height;
    }

    /// <summary>
    /// Returns tiles on the all sides of the component on the coordinates
    /// If component is set, it will act as if the component was placed at the tile.
    /// If not, it will use the one in the grid.
    /// </summary>
    public List<ComponentGridTile> GetTilesAroundComponent(int x, int z, ShipComponentController component = null) {
        if (component == null) {
            component = grid[z, x].component;
            (x, z) = GetOriginTileCoordinates(x, z);
        }
        var tiles = new List<ComponentGridTile>();
        var rules = component.placementRules;
        GetTilesOnBottom(rules, x, z, tiles);
        GetTilesOnTop(rules, x, z, tiles);
        GetTilesOnLeft(rules, x, z, tiles);
        GetTilesOnRight(rules, x, z, tiles);

        return tiles;
    }

    /// <summary>
    /// Returns tiles on the right of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnRight(ComponentPlacement rules, int x, int z, List<ComponentGridTile> output = null) {
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (x + rules.width >= width) return tiles;

        for (int i = 0; i < rules.height; i++) {
            tiles.Add(grid[z + i, x + rules.width]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the left of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnLeft(ComponentPlacement rules, int x, int z, List<ComponentGridTile> output = null) {
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (x <= 0) return tiles;

        for (int i = 0; i < rules.height; i++) {
            tiles.Add(grid[z + i, x - 1]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the bottom of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnBottom(ComponentPlacement rules, int x, int z, List<ComponentGridTile> output = null) {
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (z <= 0) return tiles;

        for (int j = 0; j < rules.width; j++) {
            tiles.Add(grid[z - 1, x + j]);
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles on the top of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnTop(ComponentPlacement rules, int x, int z, List<ComponentGridTile> output = null) {
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (z + rules.height >= height) return tiles;

        for (int j = 0; j < rules.width; j++) {
            tiles.Add(grid[z + rules.height, x + j]);
        }
        return tiles;
    }


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
}


/*
 * 
    /// <summary>
    /// Returns tiles on the right of the component on coordinates
    /// </summary>
    public List<ComponentGridTile> GetTilesOnRight(ShipComponentController component, int x, int z, List<ComponentGridTile> output = null) {
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
    public List<ComponentGridTile> GetTilesOnLeft(ShipComponentController component, int x, int z, List<ComponentGridTile> output = null) {
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
    public List<ComponentGridTile> GetTilesOnBottom(ShipComponentController component, int x, int z, List<ComponentGridTile> output = null) {
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
    public List<ComponentGridTile> GetTilesOnTop(ShipComponentController component, int x, int z, List<ComponentGridTile> output = null) {
        (x, z) = GetOriginTileCoordinates(x, z);
        var gridTile = grid[z, x];
        var tiles = output == null ? new List<ComponentGridTile>() : output;
        if (z + gridTile.ComponentHeight >= height) return tiles;

        for (int j = 0; j < gridTile.ComponentWidth; j++) {
            tiles.Add(grid[z + gridTile.ComponentHeight, x + j]);
        }
        return tiles;
    }
*/