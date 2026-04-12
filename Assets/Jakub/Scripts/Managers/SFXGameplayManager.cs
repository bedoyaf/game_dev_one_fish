using UnityEngine;


/// <summary>
/// Special Effects for the Gameplay Scene.
/// 
/// Only effects here (no transitions, scene changes etc.)
/// </summary>
public class SFXGameplayManager : MonoBehaviour
{
    [Tooltip("The player's ship")]
    [SerializeField]
    private GameObject playersShip;

    [Tooltip("The enemies's ship")]
    [SerializeField]
    private GameObject enemyShip;

    public void EnterPlayerShip()
    {
        // TODO: tween movement from the top maybe

    }

    public void EnterEnemyShip()
    {
        // move out of frame first

        // then animate down

        enemyShip.transform.position = 
            new Vector3(enemyShip.transform.position.x, enemyShip.transform.position.y, 0.6f);
    }

    public void ExitEnemyShip()
    {

        enemyShip.transform.position =
            new Vector3(enemyShip.transform.position.x, enemyShip.transform.position.y, 24f);
    }

}
