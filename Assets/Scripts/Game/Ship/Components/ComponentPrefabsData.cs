using System;
using System.Collections.Generic;
using System.Text;
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
    public List<ShipComponentController> componentPrefabs;

    public void AssignGuids() {
        //if (count != componentPrefabs.Count) {
        foreach (var component in componentPrefabs) {
            if (component != null)
                component.guid = System.Guid.NewGuid().ToString();
        }
        this.SaveScene();
        //}
    }

    public ShipComponentController GetPrefab(string guid) {
        foreach (var component in componentPrefabs) {
            if (component.guid == guid) return component;
        }
        return null;
    }

    public void Print() {
        var sb = new StringBuilder();
        foreach (var component in componentPrefabs) {
           sb.AppendLine($"{component} {component.guid}");
        }
        Debug.Log(sb.ToString());
    }
}
