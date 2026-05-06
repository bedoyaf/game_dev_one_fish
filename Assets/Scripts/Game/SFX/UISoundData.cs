using UnityEngine;

/// <summary>
/// Stores sound used for buttons in the UI.
/// </summary>
[CreateAssetMenu(fileName = "UISoundData", menuName = "Scriptable Objects/UISoundData")]
public class UISoundData : ScriptableObject
{
    public AudioClip sound;
    public float volume = 1f;
}
