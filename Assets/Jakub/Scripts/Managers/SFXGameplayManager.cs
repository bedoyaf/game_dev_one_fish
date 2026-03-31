using UnityEngine;


/// <summary>
/// Special Effects for the Gameplay Scene.
/// 
/// Only effects here (no transitions, scene changes etc.)
/// </summary>
public class SFXGameplayManager : MonoBehaviour
{
    [Tooltip("The player's ship (placeholder for now)")]
    [SerializeField]
    private GameObject playersShip;

    public void EnterPlayerShip()
    {
        // TODO: tween movement from the top maybe

    }

    public void EnterEnemyShip()
    {
        // move out of frame first

        // then animate down
    }

}
