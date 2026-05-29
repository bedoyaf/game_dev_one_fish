using System.Collections.Generic;
using System.IO;
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

// Vibe coding
public static class SoundDataSync {
    private const string ClipsFolder = "Assets/Content/Audio";
    private const string DataFolder = "Assets/ScriptableObjects/Audio";

    [MenuItem("Tools/Audio/Sync SoundData")]
    public static void Sync() {
        string[] clipGuids =
            AssetDatabase.FindAssets("t:AudioClip", new[] { ClipsFolder });

        string[] dataGuids =
            AssetDatabase.FindAssets("t:SoundData", new[] { DataFolder });

        Dictionary<AudioClip, string> existing =
            new Dictionary<AudioClip, string>();

        // Track existing SoundData
        foreach (string guid in dataGuids) {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            SoundData data =
                AssetDatabase.LoadAssetAtPath<SoundData>(path);

            if (data == null)
                continue;

            // Remove orphaned assets
            if (data.clip == null) {
                Debug.Log($"Deleting orphaned SoundData: {path}");

                AssetDatabase.DeleteAsset(path);
                continue;
            }

            existing[data.clip] = path;
        }

        // Process clips
        foreach (string guid in clipGuids) {
            string clipPath =
                AssetDatabase.GUIDToAssetPath(guid);

            AudioClip clip =
                AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);

            if (clip == null)
                continue;

            // Convert clip path to mirrored data path
            string relativePath =
                clipPath.Substring(ClipsFolder.Length + 1);

            string directory =
                Path.GetDirectoryName(relativePath);

            string fileName =
                Path.GetFileNameWithoutExtension(relativePath);

            string newFileName =
                $"{fileName}Sound.asset";

            string dataPath =
                Path.Combine(DataFolder, directory, newFileName);

            dataPath = dataPath.Replace("\\", "/");

            // Ensure folders exist
            CreateFoldersForPath(dataPath);

            // Existing asset
            if (existing.TryGetValue(clip, out string existingPath)) {
                // Move if wrong folder
                if (existingPath != dataPath) {
                    string error =
                        AssetDatabase.MoveAsset(existingPath, dataPath);

                    if (!string.IsNullOrEmpty(error)) {
                        Debug.LogError(error);
                    }
                    else {
                        Debug.Log($"Moved: {dataPath}");
                    }
                }

                continue;
            }

            // Create new asset
            SoundData newData =
                ScriptableObject.CreateInstance<SoundData>();

            newData.clip = clip;

            AssetDatabase.CreateAsset(newData, dataPath);

            Debug.Log($"Created: {dataPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("SoundData sync complete.");
    }

    private static void CreateFoldersForPath(string assetPath) {
        string directory =
            Path.GetDirectoryName(assetPath);

        directory = directory.Replace("\\", "/");

        string[] folders = directory.Split('/');

        string current = folders[0];

        for (int i = 1; i < folders.Length; i++) {
            string next =
                current + "/" + folders[i];

            if (!AssetDatabase.IsValidFolder(next)) {
                AssetDatabase.CreateFolder(current, folders[i]);
            }

            current = next;
        }
    }
}