using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(EditorCombatManagerHelper))]
public class CombatManagerHelperCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorCombatManagerHelper helper = target as EditorCombatManagerHelper;

        if (!Application.isPlaying)
        {
            return;
        }


        if (helper.canAdvance && GUILayout.Button("Next Enemy"))
        {
            helper.NextEnemy();
        }

        if(helper.canEngageEnemy && GUILayout.Button("Engage Enemy"))
        {
            helper.EngageEnemy();
        }

        if (helper.canKillEnemy && GUILayout.Button("Kill Enemy"))
        {
            helper.KillEnemy();
        }

        if (helper.canKillPlayer && GUILayout.Button("Kill Player"))
        {
            helper.KillPlayer();
        }

        if (helper.endModification && GUILayout.Button("Done modifying"))
        {
            helper.EndModifying();
        }

    }
}