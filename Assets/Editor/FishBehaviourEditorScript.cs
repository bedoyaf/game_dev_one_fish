using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FishBehaviourScript))]
public class FishBehaviourEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var values = serializedObject.FindProperty("moodSprites");

        for (int i = 0; i < values.arraySize; i++)
        {
            var element = values.GetArrayElementAtIndex(i);

            var enumName = ((Moods)i).ToString();

            EditorGUILayout.PropertyField(
                element,
                new GUIContent(enumName)
            );
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        DrawDefaultInspector();
    }
}
