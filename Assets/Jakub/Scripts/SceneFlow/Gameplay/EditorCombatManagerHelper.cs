using UnityEngine;
using static GameplayFlowManager;

public class EditorCombatManagerHelper : MonoBehaviour
{

    [SerializeField]
    private GameplayFlowManager gameplayManager;


    /// <summary>
    /// Begin combat by spawning the enemy
    /// </summary>
    public bool canAdvance => gameplayManager.stateMachine.CurrentState == GameStates.WaitingForCombat;

    public void NextEnemy()
    {
        // Some UI element (Next Battle)
        gameplayManager.LoadEnemy();
    }


    public bool canEngageEnemy => gameplayManager.stateMachine.CurrentState == GameStates.PreCombat;
    public void EngageEnemy()
    {
        // Some kind of action / UI confirmation that begins the combat
        gameplayManager.stateMachine.ChangeState(GameStates.Combat);
    }


    public bool canKillEnemy => gameplayManager.stateMachine.CurrentState == GameStates.Combat ||
                                gameplayManager.stateMachine.CurrentState == GameStates.PreCombat;
    public void KillEnemy()
    {
        // The act of the enemy's health reaching 0
        gameplayManager.EndCombat(true);
    }

    public bool canKillPlayer => gameplayManager.stateMachine.CurrentState == GameStates.Combat;
    public void KillPlayer()
    {
        // The act of the player's health reaching 0
        gameplayManager.EndCombat(false);
    }


    public bool endModification => gameplayManager.stateMachine.CurrentState == GameStates.ShipModification;
    public void EndModifying()
    {
        // Either by placing all components or maybe some UI element
        gameplayManager.ModifyShipDone();
    }

}
