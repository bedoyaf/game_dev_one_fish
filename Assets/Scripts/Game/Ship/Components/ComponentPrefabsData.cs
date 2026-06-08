using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Serves as an identification for the component.
/// Possibly some component description could be here, but not necessary.
/// I did not want to use enums or strings for identifying components
/// </summary>
[CreateAssetMenu(fileName = "ShipComponentData", menuName = "Scriptable Objects/ShipComponentData")]
public class ComponentPrefabsData : ScriptableObject
{
    //private Dictionary<ShipComponentController, string> cachedPrefabs = new();
    //public bool ContainsComponent(ShipComponentController component) {
    //    if (cachedPrefabs.Count == 0) {
    //        foreach (var cg in storedComponentPrefabs) {
    //            cachedPrefabs.Add(cg.component, cg.Guid);
    //        }
    //    }
    //    return cachedPrefabs.ContainsKey(component);
    //}

    //public string GetGuid(ShipComponentController component) {
    //    if (!ContainsComponent(component)) return "";
    //    else return cachedPrefabs[component];
    //}

    //public void Add(ShipComponentController component) {
    //    storedComponentPrefabs.Add(new ComponentWithGUID() { Guid = component.guid, component = component });
    //    cachedPrefabs.Add(component, component.guid);

    //    storedComponentPrefabs.RemoveAll(x => x.component == null);
    //}

    //public List<ComponentWithGUID> storedComponentPrefabs;

    //[Serializable]
    //public class ComponentWithGUID {
    //    public string Guid;
    //    public ShipComponentController component;
    //}
    [TextArea(3, 10)]
    public string useDescription;

    public List<ShipComponentController> componentPrefabs;

    public void AssignGuids()
    {

        int c = 0;

        //if (count != componentPrefabs.Count) {
        // var visited = new HashSet<string>();
        foreach (var component in componentPrefabs)
        {
            if (component != null)
            {
                var path = AssetDatabase.GetAssetPath(component);

                var root = PrefabUtility.LoadPrefabContents(path);

                var comp = root.GetComponent<ShipComponentController>();
                // if (visited.Contains(component.guid)) {
                // Debug.Log($"Duplicate found {component.guid}");
                comp.guid = "" + (c++); //  System.Guid.NewGuid().ToString();

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);

                
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(component);
#endif

                /* To mark as modified 
                if (PrefabUtility.IsPartOfVariantPrefab(component))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
                */
                // }

                // visited.Add(component.guid);
            }
        }
        this.SaveScene();

        //* actually look if done what wanted to
        var list = new List<string>();
        foreach (var component in componentPrefabs)
        {
            list.Add(component.guid);
        }
        list.Sort();

        var duplicates = list
        .GroupBy(x => x)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);

        foreach (var item in duplicates)
        {
            Debug.Log(item);
        }

    }

    public ShipComponentController GetPrefab(string guid)
    {
        foreach (var component in componentPrefabs)
        {
            if (component.guid == guid) return component;
        }
        return null;
    }

    // Loads all prefabs in assets/prefabs/components and adds those that are missing
    public void LoadComponentPrefabs()
    {
#if UNITY_EDITOR
        // Guid je trochu pøetížený pojem
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Components" });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            ShipComponentController comp = AssetDatabase.LoadAssetAtPath<ShipComponentController>(path);

            if (!componentPrefabs.Contains(comp))
            {
                componentPrefabs.Add(comp);
            }
        }

        AssignGuids();
#endif
    }

    public void ClearAndLoad()
    {
        componentPrefabs.Clear();
    }

    public void Print()
    {
        var sb = new StringBuilder();
        foreach (var component in componentPrefabs)
        {
            sb.AppendLine($"{component} {component.guid}");
        }
        Debug.Log(sb.ToString());
    }
}
