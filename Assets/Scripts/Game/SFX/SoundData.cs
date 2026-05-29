using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Stores sound used for buttons in the UI.
/// </summary>
[CreateAssetMenu(fileName = "UISoundData", menuName = "Scriptable Objects/UISoundData")]
public class SoundData : ScriptableObject
{
    [FormerlySerializedAs("sound")]
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range (0.5f, 1.5f)]
    public float pitch = 1f;

    public bool doNotChangeName = false; // If the name should automatically be set to name of the clip
}
