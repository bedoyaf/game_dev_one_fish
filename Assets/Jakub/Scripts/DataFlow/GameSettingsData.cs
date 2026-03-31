using UnityEngine;


// TODO: fill out with actual settings
// e.g   Audio levels, graphics?, subtitles? etc. 

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Tooltip("Scene transition object prefab")]
    public GameObject sceneTransitionPrefab;

}
