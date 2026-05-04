using UnityEngine;
using static GameplayFlowManager;




//HEAVILY BROKEN BY FILIP, SORRY:(
public class EditorCombatManagerHelper : MonoBehaviour
{

    [SerializeField]
    private GameplayFlowManager gameplayManager;


    /// <summary>
    /// Begin combat by spawning the enemy
    /// </summary>
    public bool canAdvance => gameplayManager.stateMachine.CurrentStateKey == GameStates.WaitingForCombat;

    public void NextEnemy()
    {
        // Some UI element (Next Battle)
      //  gameplayManager.LoadEnemy();
    }


    public bool canEngageEnemy => gameplayManager.stateMachine.CurrentStateKey == GameStates.PreCombat;
    public void EngageEnemy()
    {
        // Some kind of action / UI confirmation that begins the combat
        gameplayManager.stateMachine.ChangeState(GameStates.Combat);
    }


    public bool canKillEnemy => gameplayManager.stateMachine.CurrentStateKey == GameStates.Combat ||
                                gameplayManager.stateMachine.CurrentStateKey == GameStates.PreCombat;
    public void KillEnemy()
    {
        // The act of the enemy's health reaching 0
        //gameplayManager.EndCombat(true);
    }

    public bool canKillPlayer => gameplayManager.stateMachine.CurrentStateKey == GameStates.Combat;
    public void KillPlayer()
    {
        // The act of the player's health reaching 0
      //  gameplayManager.EndCombat(false);
    }


    public bool endModification => gameplayManager.stateMachine.CurrentStateKey == GameStates.ShipModification;
    public void EndModifying()
    {
        // Either by placing all components or maybe some UI element
        gameplayManager.ModifyShipDone();
    }

}
