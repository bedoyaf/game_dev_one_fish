using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShipData))]
public class ShipDataEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        ShipData data = target as ShipData;

        if (GUILayout.Button("Auto assign drops")) {
            data.UpdatePossibleDrops();
        }

        if (GUILayout.Button("Reset")) {
            data.componentGrid = null;
        }
    }
}