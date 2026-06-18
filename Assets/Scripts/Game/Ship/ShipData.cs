using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "Scriptable Objects/ShipData")]
public class ShipData : ScriptableObject {
    /// <summary>
    /// Should the drops be automatically updated to all components on the ship after any change?
    /// </summary>
    [SerializeField] private bool autoUpdateDrops = true;

    /// <summary>
    /// Numerical difficulty of the enemy. I suggest range from 0 to 10.
    /// </summary>
    public int enemyDifficulty;

    /// <summary>
    /// What can drop from the ship
    /// Disable <see cref="autoUpdateDrops"/> if you want to set this manually
    /// </summary>
    public List<ShipComponentController> possibleDrops;

    [SerializeField]
    private ComponentGrid _componentGrid;

    
    public string shipName = "defaultName";

    public int bannerIndex = -1;

    /// <summary>
    /// The stored component grid. All things inside are references to prefabs, not instances.
    /// </summary>
    public ComponentGrid componentGrid {
        get {
            this.SaveScene();
            return _componentGrid;
        }
        set {
            _componentGrid = value;
            this.SaveScene();
        }
    }

    /// <summary>
    /// Enables the component grid to be accessed like a matrix
    /// </summary>
    /// <returns></returns>
    public ComponentGridTile this[int z, int x] {
        get {
            return _componentGrid[z, x];
        }
        set {
            _componentGrid[z, x] = value;
        }
    }

    /// <summary>
    /// Builds ship from data.
    /// Instantializes the components under the component parent.
    /// </summary>
    /// <param name="componentParent"></param>
    /// <returns>The component grid.</returns>
    public ComponentGrid BuildShip(Transform componentParent) {
        var shipGrid = new ComponentGrid(_componentGrid.width, _componentGrid.height, _componentGrid.placeholderPrefab, false, componentParent);
        _componentGrid.CopyComponentGrid(shipGrid);

        // If someone forgets to make a solid component
        if (!_componentGrid.ContainsSolid()) {
            shipGrid.SetEverythingSolid();
        }

        UpdatePossibleDrops();
        this.SaveScene();

        return shipGrid;
    }

    /// <summary>
    /// Auto update what can drop from the ship after it has been accessed
    /// </summary>
    public void UpdatePossibleDrops() {
        if (!autoUpdateDrops || _componentGrid == null) return;

        possibleDrops = _componentGrid.GetAllUniqueComponentPrefabs();
        possibleDrops.RemoveAll(x => x.placementRules.solid); // Remove the cabin from possible drops
        this.SaveScene();
    }


    /// <summary>
    /// Builds ship from data at position
    /// </summary>
    /// <param name="position">Left bottom corner of the grid.</param>
    /// <param name="componentParent">The object in hierarchy under which the ship components will be instantiated.</param>
    /// <param name="placeholder">Placeholder component, if needed from builder.</param>
    /// <returns>List of references to instantiated components.</returns>
    //    public List<ShipComponentController> BuildShip_old(Vector3 position, Transform componentParent, ShipComponentController placeholder = null) {
    //        List<ShipComponentController> createdComponents = new();
    //        for (int i = 0; i < componentGrid.height; i++) {
    //            for (int j = 0; j < componentGrid.width; j++) {
    //                var gridTile = componentGrid[i, j];
    //                var component = gridTile.component;
    //                if (gridTile.isPlaceholder || gridTile.hasOffset) { 
    //                    continue;
    //                }
    //                // If in editor, make sure objects stay as prefabs
    //#if UNITY_EDITOR
    //                if (Application.isPlaying) {
    //                    var comp = Instantiate(component, new Vector3(j, 0, i) + position, Quaternion.identity, componentParent);
    //                    createdComponents.Add(comp);
    //                }
    //                else {
    //                    var obj = PrefabUtility.InstantiatePrefab(component, componentParent) as ShipComponentController;
    //                    obj.transform.position = new Vector3(j, 0, i) + position;
    //                    createdComponents.Add(obj);
    //                }
    //#else
    //                var comp = Instantiate(component, new Vector3(j, 0, i) + position, Quaternion.identity, componentParent);
    //                createdComponents.Add(comp);
    //#endif
    //            }
    //        }

    //        return createdComponents;
    //    }

}


public static class EditorHelper {
    public static void SaveScene(this Object a) {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(a);
#endif
    }
}

