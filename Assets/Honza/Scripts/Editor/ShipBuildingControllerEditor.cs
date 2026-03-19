using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShipBuildingController))]
public class ShipBuildingControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ShipBuildingController shipBuilder = target as ShipBuildingController;

        
        if (Application.isPlaying && GUILayout.Button("Toggle placeholders")) {
            shipBuilder.TogglePlaceholders();
        }
    }
}