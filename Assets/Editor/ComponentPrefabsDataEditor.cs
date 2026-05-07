using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComponentPrefabsData))]
public class ComponentPrefabsDataEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ComponentPrefabsData prefabsData = target as ComponentPrefabsData;

        if (GUILayout.Button("Reset GUIDs")) {
            prefabsData.AssignGuids();
        }

        //if (GUILayout.Button("Print")) {
        //    prefabsData.Print();
        //}
    }
}