using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventController))]
public class EventControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EventController controller = target as EventController;

        if (GUILayout.Button("Create UI")) {
            controller.DisplayUI();
        }

        if (GUILayout.Button("Hide UI")) {
            controller.HideUI();
        }

        if (GUILayout.Button("Print events")) {
            controller.PrintEvents();
        }
    }
}