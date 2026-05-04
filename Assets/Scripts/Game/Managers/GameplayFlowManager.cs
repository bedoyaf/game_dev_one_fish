using System.Collections.Generic;
using UnityEngine;
using static MapController;



/// <summary>
/// Manages everything that is happening inside the gameplay scene.
/// Instanced - for easier everything...
///     Using Global GameManager data
///     On... functions are called to announce change in flow
///     other functions manage the game itself
///     example
///     OnCombatEnd announces the combat ended
///     EndCombat, does the things relevent to ending comabt for the flow, such as changing state
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
    private ShipController enemyShip;

    [SerializeField]
    private EventController eventController;

    [SerializeField]
    private MapController mapController;

    [SerializeField] private CombatController combatController;
    [SerializeField] private RewardController rewardController;

    // Some access to player ship is needed
    public ShipController PlayerShip => playerShip;
    public ShipController EnemyShip => enemyShip;

    void Awake()
    {
        GameManager.Instance.SetGameplayFlowInstance(this);

        var states = new Dictionary<GameStates, IGameState>
        {
            { GameStates.WaitingForCombat, new WaitingForCombatState(this) },
            { GameStates.PreCombat, new PreCombatState(this) },
            { GameStates.Combat, new CombatState(this) },
            { GameStates.Event, new EventState(this) },
            { GameStates.RewardSelection, new RewardSelectionState(this) },
            { GameStates.ShipModification, new ShipModificationState(this) },
            { GameStates.MapSelection, new MapSelectionState(this) },
            { GameStates.GameOver, new GameOverState(this) },
        };

        stateMachine = new GameStateMachine(states);
        stateMachine.ChangeState(GameStates.MapSelection);
    }

    // TODO: state machine for the game phases
    public enum GameStates
    {
        Event,            // Event screen is up
        WaitingForCombat, // Entry State (the ui is showing map selection, can toggle to see ship in aquarium)
        
        PreCombat,        // Enemy is entering, but fight has not commenced yet, after a some time -> begin
        Combat,           // Enemy is in the scene, fight is fully going

        RewardSelection,
        ShipModification,

        MapSelection,
        GameOver
    }

    public class GameStateMachine
    {
        private Dictionary<GameStates, IGameState> states;
        private IGameState currentState;

        public GameStates CurrentStateKey { get; private set; }

        public GameStateMachine(Dictionary<GameStates, IGameState> states)
        {
            this.states = states;
            currentState = states[GameStates.WaitingForCombat];
            currentState.Enter();
        }

        public void ChangeState(GameStates newState)
        {
            if (CurrentStateKey == newState) return;

            currentState?.Exit();

            currentState = states[newState];
            CurrentStateKey = newState;

            currentState.Enter();
        }
    }

    public GameStateMachine stateMachine;


    // ----------------------------------------------------------------------------

    public void OpenShipBuilder()
    {
        playerShip.GiveControlToEditor(rewardController.storedComponents);
        //should switch to building ship
    }

    public void CloseShipEditor()
    {
        playerShip.RemoveControlFromEditor();
        //should hide the shipbuilding
    }

    public void OpenMapController() {
        mapController.DisplayChoices();
    }

    public void CloseMapController(MapChoiceData choiceData) {

        // Event
        if (!choiceData.fight) {
            stateMachine.ChangeState(GameStates.Event);
            return;
        }

        // Normal/elite combat - TODO difficulty
        if (!choiceData.boss) {
            Fight(choiceData.difficulty);
            return;
        }

        // TODO boss battle
    }

    public void OpenEventController() {
        eventController.NextEvent();
    }

    /// <summary>
    /// Called when the event does not change game state.
    /// </summary>
    public void CloseEventController() {
        stateMachine.ChangeState(GameStates.MapSelection);
    }

    public void UnloadEnemy()
    {
        combatController.UnLoadEnemyShip();
        sfx.ExitEnemyShip();
    }


    // TODO: pass argument of the loaded ship etc..
    public void LoadEnemy()
    {
        combatController.LoadEnemyShip();
        sfx.EnterEnemyShip();
    }
    public void LoadPlayer()
    {
        combatController.LoadPlayerShip();
    }


    public void EndCombat()
    {

    }

    // TODO: properly 
    public void ModifyShipDone()
    {
        playerShip.RemoveControlFromEditor();

        stateMachine.ChangeState(GameStates.WaitingForCombat);
    }

    public void EventDone() {
        stateMachine.ChangeState(GameStates.WaitingForCombat);
    }

    public void EnterCombat()
    {
        combatController.StartCombat();
    }


    public void ShowRewards()
    {
        rewardController.StartChoosing(enemyShip);
    }

    // EVENTS that trigger the change of state


    public void OnCombatEnd()
    {
        // kill the enemy / remove them
        if (combatController.playerWon)
        {
            // Spawn loot
            stateMachine.ChangeState(GameStates.RewardSelection);
        }
        else
        {
            stateMachine.ChangeState(GameStates.GameOver);
            // Display Game Over (TODO)
        }
    }

    public void OnShowRewardEnd()
    {
        stateMachine.ChangeState(GameStates.ShipModification);
    }

    //Helpers, later will be scraped

    public void AdvanceState()
    {
        if(stateMachine.CurrentStateKey == GameStates.Combat)
        {
            enemyShip.GetMainCabin().TakeDamage(1000);
            return;
        }
        var next = GetNextState(stateMachine.CurrentStateKey);
        stateMachine.ChangeState(next);
    }

    private GameStates GetNextState(GameStates current)
    {
        return current switch
        {
            GameStates.WaitingForCombat => GameStates.PreCombat,
            GameStates.PreCombat => GameStates.Combat,
            GameStates.Combat => GameStates.RewardSelection,
            GameStates.RewardSelection => GameStates.ShipModification,
            GameStates.ShipModification => GameStates.MapSelection,
            GameStates.MapSelection => GameStates.WaitingForCombat,

            _ => GameStates.WaitingForCombat
        };
    }

    // ----------------------------------------------------
    public void Fight(int difficulty) {
        Debug.Log($"Fight called {difficulty}");
        combatController.AssignEnemyByDifficulty(difficulty);
        stateMachine.ChangeState(GameStates.PreCombat);
    }

    public void Fight(ShipData enemy) {
        Debug.Log($"Fight called {enemy}");
        combatController.AssignEnemy(enemy);
        stateMachine.ChangeState(GameStates.PreCombat);
    }

    public void NewComponent(ShipComponentController component) {
        Debug.Log($"Adding component {component}");
        rewardController.AssignComponent(component);
        stateMachine.ChangeState(GameStates.ShipModification);
    }

    void OnGUI()
    {
        float width = 180;
        float height = 40;

        float x = Screen.width - width - 10;
        float y = Screen.height - height - 10;

        GUI.Box(new Rect(x, y - 50, width, 30), "State: " + stateMachine.CurrentStateKey);

        if (GUI.Button(new Rect(x, y, width, height), "Advance State"))
        {
            AdvanceState();
        }

        if (stateMachine.CurrentStateKey == GameStates.GameOver)
        {
            if (GUI.Button(new Rect(500, 400, 150, 40), "RESTART"))
            {
                stateMachine.ChangeState(GameStates.WaitingForCombat);
            }
        }
    }

    // TODO: (if will ever do) map selection
}

