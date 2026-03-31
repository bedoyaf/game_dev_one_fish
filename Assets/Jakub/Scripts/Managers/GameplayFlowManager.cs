using UnityEngine;



/// <summary>
/// Manages everything that is happening inside the gameplay scene.
/// Instanced - for easier everything...
///     Using Global GameManager data
/// </summary>
public class GameplayFlowManager : MonoBehaviour
{
    [Tooltip("SFX Manager for the gameplay scene")]
    [SerializeField]
    private SFXGameplayManager sfx;


    // TODO: pass argument of the loaded ship etc..
    public void LoadEnemy()
    {
        // TODO: Add the enemy ship 

        sfx.EnterEnemyShip();
    }

}
