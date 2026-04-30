using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(EventData))]
public class EventDataEditor : Editor {
    //public override void OnInspectorGUI() {
    //    base.OnInspectorGUI();
    //    EventData data = target as EventData;

    //    if (GUILayout.Button("Print Event")) {
    //        data.PrintData();
    //    }
    //}
    // Wibe coding
    public override VisualElement CreateInspectorGUI() {
        var root = new VisualElement();

        var property = serializedObject.GetIterator();

        if (property.NextVisible(true)) {
            do {
                var field = new PropertyField(property.Copy());
                field.Bind(serializedObject);
                root.Add(field);

            } while (property.NextVisible(false));
        }

        //var button = new Button(() => {
        //    var eventData = (EventData)target;

        //    // Call your function here
        //    eventData.PrintData();

        //    // Mark dirty if it modifies data
        //    //EditorUtility.SetDirty(eventData);
        //}) {
        //    text = "Print Data"
        //};
        //button.style.backgroundColor = new Color(1, 0, 0, 0.3f);

        //root.Add(button);

        return root;
    }
}