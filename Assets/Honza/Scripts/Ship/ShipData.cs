using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "ShipData", menuName = "Scriptable Objects/ShipData")]
public class ShipData : ScriptableObject
{
    /// <summary>
    /// Width of the grid
    /// </summary>
    public int width;

    /// <summary>
    /// Height of the grid
    /// </summary>
    public int height;

    /// <summary>
    /// 2D map of the components stored in 1D, because it is serializable like that
    /// </summary>
    public List<ShipComponentController> components;

    /// <summary>
    /// Builds ship from data at position
    /// </summary>
    /// <param name="position">Left bottom corner of the grid.</param>
    /// <param name="componentParent">The object in hierarchy under which the ship components will be instantiated.</param>
    /// <param name="placeholder">Placeholder component, if needed from builder.</param>
    /// <returns>List of references to instantiated components.</returns>
    public List<ShipComponentController> BuildShip(Vector3 position, Transform componentParent, ShipComponentController placeholder = null) {
        List<ShipComponentController> createdComponents = new();
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                var component = components[i * width + j];
                if (component == null) {
                    if (placeholder != null) {
                        component = placeholder;
                    }
                    else {
                        continue;
                    }
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
