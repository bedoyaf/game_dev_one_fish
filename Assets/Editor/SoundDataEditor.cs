using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using NUnit.Framework.Constraints;

[CustomEditor(typeof(SoundData))]
public class SoundDataEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var sound = target as SoundData;

        if (GUILayout.Button("Play sound")) {
            var source = AudioManager.Instance.PlaySFX(sound);
            AudioManager.Instance.StartCoroutine(DestroyThis(sound, source.gameObject));
        }
    }

    private IEnumerator DestroyThis(SoundData sound, GameObject gameObject) {
        yield return new WaitForSeconds(sound.clip.length + 1);
        gameObject.SmartDestroy();
    }
}