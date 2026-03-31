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

    void Awake()
    {
        GameManager_Jakub.Instance.SetGameplayFlowInstance(this);
    }

    // TODO: pass argument of the loaded ship etc..
    public void LoadEnemy()
    {
        // TODO: Add the enemy ship 

        sfx.EnterEnemyShip();
    }


    public void EndCombat()
    {
        // kill the enemy / remove them

        // spawn the component
    }

    // TODO: properly 
    private void ModifyShip()
    {

    }
    

    // TODO: (if will ever do) map selection
}
