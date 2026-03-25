using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ComponentGrid;

[CreateAssetMenu(fileName = "ShipData", menuName = "Scriptable Objects/ShipData")]
public class ShipData : ScriptableObject {
    /// <summary>
    /// The stored component grid. All things inside are references to prefabs, not instances.
    /// </summary>
    public ComponentGrid componentGrid {
        get {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return _componentGrid;
        }
        set {
            _componentGrid = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    [SerializeField]
    private ComponentGrid _componentGrid;

    /// <summary>
    /// Enables the component grid to be accessed like a matrix
    /// </summary>
    /// <returns></returns>
    public ComponentGridTile this[int z, int x] {
        get {
            return componentGrid[z, x];
        }
        set {
            componentGrid[z, x] = value;
        }
    }

    /// <summary>
    /// Builds ship from data at position
    /// </summary>
    /// <param name="position">Left bottom corner of the grid.</param>
    /// <param name="componentParent">The object in hierarchy under which the ship components will be instantiated.</param>
    /// <param name="placeholder">Placeholder component, if needed from builder.</param>
    /// <returns>List of references to instantiated components.</returns>
    public List<ShipComponentController> BuildShip(Vector3 position, Transform componentParent, ShipComponentController placeholder = null) {
        List<ShipComponentController> createdComponents = new();
        for (int i = 0; i < componentGrid.height; i++) {
            for (int j = 0; j < componentGrid.width; j++) {
                var gridTile = componentGrid[i, j];
                var component = gridTile.component;
                if (gridTile.isPlaceholder || gridTile.hasOffset) {
                    continue;
                }
                // If in editor, make sure objects stay as prefabs
#if UNITY_EDITOR
                if (Application.isPlaying) {
                    var comp = Instantiate(component, new Vector3(j, 0, i) + position, Quaternion.identity, componentParent);
                    createdComponents.Add(comp);
                }
                else {
                    var obj = PrefabUtility.InstantiatePrefab(component, componentParent) as ShipComponentController;
                    obj.transform.position = new Vector3(j, 0, i) + position;
                    createdComponents.Add(obj);
                }
#else
                var comp = Instantiate(component, new Vector3(j, 0, i) + position, Quaternion.identity, componentParent);
                createdComponents.Add(comp);
#endif
            }
        }

        return createdComponents;
    }

}
