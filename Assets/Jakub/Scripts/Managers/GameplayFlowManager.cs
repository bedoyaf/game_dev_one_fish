using System.Collections.Generic;
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


    [Tooltip("The player's ship")]
    [SerializeField]
    private ShipController playerShip;

    [Tooltip("The enemy ship")]
    [SerializeField]
    private GameObject enemyShip;



    void Awake()
    {
        GameManager_Jakub.Instance.SetGameplayFlowInstance(this);

        stateMachine = new(this);
    }

    // TODO: state machine for the game phases
    public enum GameStates
    {
        WaitingForCombat, // Entry State
        PreCombat,        // Enemy is entering, but fight has not commenced yet
        
        Combat,
        Event,

        RewardSelection,
        ShipModification,

    }

    public class GameStateMachine
    {
        private readonly GameplayFlowManager manager;
        public GameStateMachine(GameplayFlowManager manager)
        {
            this.manager = manager;
        }

        public GameStates CurrentState { get; private set; }

        public void ChangeState(GameStates newState)
        {
            // NOTE: maybe make this less stupid XD

            CurrentState = newState;

            switch (newState)
            {
                case GameStates.WaitingForCombat:
                    manager.UnloadEnemy();
                    break;
                
                case GameStates.PreCombat:
                    manager.LoadingEnemy();
                    break;
                
                case GameStates.Combat:
                    Debug.Log("So... you have chosen, death.");
                    break;
                
                    // TODO
                case GameStates.Event:
                    break;

                case GameStates.RewardSelection:
                    // TODO: spawn the loot, based on some loot table
                    // sfx -> show the ui etc.
                    ChangeState(GameStates.ShipModification);

                    break;

                case GameStates.ShipModification:
                    
                    // Enter editor mode
                    manager.playerShip.GiveControlToEditor();
                    
                    break;

                default:
                    break;
            }

        }
    }

    public GameStateMachine stateMachine;


    // ----------------------------------------------------------------------------

    private void UnloadEnemy()
    {
        sfx.ExitEnemyShip();
    }


    // TODO: pass argument of the loaded ship etc..
    public void LoadEnemy()
    {
        // TODO: Add the enemy ship (more precisely change the shipData)
        // enemyShip. as Ship Controller ? some enemy base script

        stateMachine.ChangeState(GameStates.PreCombat);
    }

    private void LoadingEnemy()
    {
        sfx.EnterEnemyShip();
    }



    public void EndCombat(bool playerWon)
    {
        // kill the enemy / remove them
        if(playerWon)
        {
            // Spawn loot
            stateMachine.ChangeState(GameStates.RewardSelection);
        } else
        {
            // Display Game Over (TODO)
        }

    }

    // TODO: properly 
    public void ModifyShipDone()
    {
        playerShip.RemoveControlFromEditor();

        stateMachine.ChangeState(GameStates.WaitingForCombat);
    }
    

    // TODO: (if will ever do) map selection
}
