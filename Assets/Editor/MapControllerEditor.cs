using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        MapController controller = target as MapController;

        if (GUILayout.Button("Create UI")) {
            controller.DisplayMap();
        }

        //if (GUILayout.Button("Hide UI")) {
        //    controller.HideUI();
        //}

        //if (GUILayout.Button("Print events")) {
        //    controller.PrintEvents();
        //}
    }
}