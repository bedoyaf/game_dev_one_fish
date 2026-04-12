using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShipController))]
public class ShipControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ShipController ship = target as ShipController;

        if (GUILayout.Button("Build Ship")) {
            ship.BuildShip();
        }

        if (!Application.isPlaying) {
            return;
        }

        if (ship.shipEditor == null) return;

        if (GUILayout.Button("Swich to editor")) {
            ship.GiveControlToEditor();
        }

        if (GUILayout.Button("Switch from editor")) {
            ship.RemoveControlFromEditor();
        }
    }
}