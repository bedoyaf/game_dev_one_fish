using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static MapController;
using static UnityEngine.Timeline.DirectorControlPlayable;



/// <summary>
/// Manages everything that is happening inside the gameplay scene.
/// Instanced - for easier everything...
///     Using Global GameManager data
///     On... functions are called to announce change in flow
///     other functions manage the game itself
///     example
///     OnCombatEnd announces the combat ended
///     EndCombat, does the things relevant to ending combat for the flow, such as changing state
/// </summary>
public class GameplayFlowManager : MonoBehaviour
{
    public bool ShowDebugUI = false;

    [Tooltip("SFX Manager for the gameplay scene")]
    public SFXGameplayManager sfx;


    [Tooltip("The player's ship")]
    [SerializeField]
    public ShipController playerShip;

    [Tooltip("The enemy's ship / for compatibility")]
    [SerializeField]
    private ShipController enemyShip;

    [SerializeField]
    private EventController eventController;

    [SerializeField]
    public MapController mapController;


    public TutorialController tutorialController;
    public CombatController combatController;
    public RewardController rewardController;

    public MouseController mouseController;

    public GameUIScript gameUi;
    public SoundData startGameSound;
    public bool debugSkipTutorial;

    // Some access to player ship is needed
    public ShipController PlayerShip => playerShip;
    public ShipController EnemyShip => enemyShip;

    public bool InCombat => stateMachine.CurrentStateKey == GameStates.Combat;
    public bool tutorialRunning { get; private set; } = false;

    [SerializeField] private SoundData combatMusic;

    [SerializeField] private float timeTillEndingCutscene = 10f;
    [SerializeField] private InputActionReference toggleDebug;

    private void OnEnable()
    {
        toggleDebug.action.performed += toggleDebugBool;
        toggleDebug.action.Enable();
    }

    private void toggleDebugBool(InputAction.CallbackContext context)
    {
        ShowDebugUI = !ShowDebugUI;
    }

    private void OnDisable()
    {
        toggleDebug.action.performed -= toggleDebugBool;
        toggleDebug.action.Disable();
    }


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
            { GameStates.Repairs, new RepairState(this) },
            { GameStates.Ending, new GameEndingState(this) },
        };

        stateMachine = new GameStateMachine(states);
        //  stateMachine.ChangeState(GameStates.MapSelection);

        // EnterTutorial();

        mapController.StartMap();
    }

    void Start()
    {
        AudioManager.Instance.PlayMusic(combatMusic);
        // Just debug if I want to start with some better ship
        if (debugSkipTutorial) {
            stateMachine.ChangeState(GameStates.MapSelection);
        }
        else {
            if (!GameManager.Instance.TutorialFinished) {
                EnterTutorial();
            }
            else {
                StartCoroutine(StartNormalGame());
            }
        }
    }

    private IEnumerator StartNormalGame() {
        yield return null;
        ChangePlayerShip();
        EnterFirstGameplay();
        sfx.SuperSpeedEnemyShipExit();
    }

    // TODO: state machine for the game phases
    public enum GameStates
    {
        Event,            // Event screen is up
        WaitingForCombat, // Entry State (the ui is showing map selection, can toggle to see ship in aquarium)
        
        PreCombat,        // Enemy is entering, but fight has not commenced yet, after a some time -> begin
        Combat,           // Enemy is in the scene, fight is fully going

        RewardSelection,  // Rewards presented -> can only choose between them
        ShipModification, // Modifying the ship with the chose components -> can abord

        MapSelection,     // Map is up, can select where to go next
        GameOver,         // Game is over -> can restart
    
        Repairs,           // Repairing
        Ending
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
        sfx.EncounterTransition("Building stage");
        playerShip.GiveControlToEditor(rewardController.storedComponents);
        //should switch to building ship
    }

    public void CloseShipEditor()
    {
        playerShip.RemoveControlFromEditor();
        //should hide the shipbuilding
    }

    public void OpenMapController() {
        mapController.DisplayMap();
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

        if (choiceData.boss)
        {
            Debug.Log("Boss fight");
            Fight(combatController.boss);
            return;
        }
        // TODO boss battle
    }

    public void OpenEventController() {
        eventController.NextEvent(mapController.CurrentNode);
    }

    /// <summary>
    /// Called when the event does not change game state.
    /// </summary>
    public void CloseEventController() {
        stateMachine.ChangeState(GameStates.MapSelection);
    }

    
    public void EnterRepairsMode()
    {
        sfx.EncounterTransition("Repaire stage", false);
        stateMachine.ChangeState(GameStates.Repairs);
    }

    public void ExitRepairsMode()
    {
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
        enemyShip = combatController.LoadEnemyShip(playerShip);

        if (!tutorialRunning)
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


    public void PlayGameEndingCutscene()
    {
        StartCoroutine(LoadEndingAfterDelay());
    }

    private IEnumerator LoadEndingAfterDelay()
    {

        yield return new WaitForSeconds(timeTillEndingCutscene);

        GameManager.Instance.TransitionScene("EndingCutscenesScene");
    }

    // Called to stop modifying the ship 
    // Will toss unused components 
    public void StopModifying()
    {
        //if(tutorialRunning)
        //{
        //    stateMachine.ChangeState(GameStates.Repairs);
        //    EndTutorial();
        //    return;
        //}
        stateMachine.ChangeState(GameStates.MapSelection);
    }

    //private bool skippedTutorial = false;
    public void SkipTutorial()
    {
        //skippedTutorial = true;
        EndTutorial();
        //change TODO
        //enemyShip.GetMainCabin()?.TakeDamage(1000);
        //EndTutorial(); // not needed, combat end will handle it
    }

    public void EndTutorial() {
        AudioManager.Instance.PlaySFX(startGameSound);
        GameManager.Instance.OnTutorialFinished();

    }
    /*private void EndTutorial()
    {
        tutorialRunning = false;
        ChangePlayerShip();


        tutorialController.EndTutorial();
        EnterFirstGameplay();
    }*/

    public void OnRegularTutorialEnd() {
        EndTutorial();
        //EndTutorial();
        //stateMachine.ChangeState(GameStates.ShipModification);
    }

    private void ChangePlayerShip()
    {
        playerShip.shipData = combatController.EmptyPlayerShip;
        playerShip.BuildShip();
    }

    public void EnterFirstGameplay()
    {
        Debug.Log("Entering first gameplay");
        var components = combatController.componentGeneratorSO.GenerateComponentList();

        foreach (var compPrefab in components)
        {
            var instancedComp = Instantiate(compPrefab);

            combatController.AddComponentLoot(instancedComp, false);
        }
        stateMachine.ChangeState(GameStates.ShipModification);
    }

    // EVENTS that trigger the change of state


    public void OnCombatEnd()
    {
        //if(skippedTutorial)
        //{
        //    skippedTutorial = false;
        //    EndTutorial();
        //    //return;
        //}
        // kill the enemy / remove them
        if (combatController.playerWon)
        {
            //if(combatController.bossKilled)
            //{
            //    OnBossDeath();
            //    return;
            //}
            // Spawn loot
            //sfx.CombatEndTransition(true, () => { stateMachine.ChangeState(GameStates.RewardSelection); });
            sfx.CombatEndTransition(true, () => {
                Debug.Log("Entering ship mod");
                if (!tutorialRunning && !combatController.bossKilled)
                    stateMachine.ChangeState(GameStates.ShipModification);
                else if (combatController.bossKilled) {
                    OnBossDeath();
                }

                Debug.Log($"Current state {stateMachine.CurrentStateKey}");
            });
        }
        else
        {
            // Display Game Over (TODO)
            sfx.CombatEndTransition(false, () => { stateMachine.ChangeState(GameStates.GameOver); });
            sfx.SetFishFace(Moods.Dead);
        }

    }

    public void OnBossDeath()
    {
        stateMachine.ChangeState(GameStates.Ending);
    }

    public void ShowGameOver()
    {
        gameUi.ShowGameOver();
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
            GameStates.Combat => GameStates.ShipModification,
            GameStates.RewardSelection => GameStates.ShipModification,
            GameStates.ShipModification => GameStates.MapSelection,
            GameStates.MapSelection => GameStates.WaitingForCombat,
            GameStates.Repairs => GameStates.MapSelection,

            _ => GameStates.WaitingForCombat
        };
    }

    // ----------------------------------------------------
    public void Fight(int difficulty) {
        Debug.Log($"Fight called {difficulty}");
        var data = combatController.AssignEnemyByDifficulty(difficulty);
        stateMachine.ChangeState(GameStates.PreCombat);
        sfx.CombatStartTransition(
            data.shipName, 
            () => { stateMachine.ChangeState(GameStates.Combat); },
            data.bannerIndex);
    }

    public void Fight(ShipController enemyPrefab) {
        Debug.Log($"Fight called {enemyPrefab.shipData}");
        combatController.AssignEnemy(enemyPrefab);
        stateMachine.ChangeState(GameStates.PreCombat);
        sfx.CombatStartTransition(
            enemyPrefab.shipData.shipName, 
            () => { stateMachine.ChangeState(GameStates.Combat); },
            enemyPrefab.shipData.bannerIndex);
    }

  /*  public void Fight(ShipController enemyPrefab)
    {
        Debug.Log($"Fight called {enemyPrefab.shipData}");
        combatController.AssignEnemy(enemyPrefab);
        stateMachine.ChangeState(GameStates.PreCombat);
        sfx.CombatStartTransition(enemyPrefab.shipData.shipName, () => { stateMachine.ChangeState(GameStates.Combat); });
    }*/

    // TODO this is ugly - the same thing is already in map controller.
    public void Fight(bool elite) {
        float difficulty = elite ? mapController.EliteDifficulty : mapController.CurrentDifficulty;
        Fight((int)difficulty);
    }

    public void NewComponent(ShipComponentController component) {
        Debug.Log($"Adding component {component}");
        rewardController.AssignComponent(component);
        stateMachine.ChangeState(GameStates.ShipModification);
    }

    public void EnterTutorial()
    {
        tutorialRunning = true;
        Fight(combatController.tutorialShip);
        tutorialController.StartTutorial();
    }


    void OnGUI()
    {
        if (!ShowDebugUI) return;

        float width = 180;
        float height = 40;

        float x = Screen.width - width - 10;
        float y = Screen.height - height - 10;

        GUI.Box(new Rect(x, y - 50, width, 30), "State: " + stateMachine.CurrentStateKey);

        //if (GUI.Button(new Rect(x, y, width, height), "Advance State"))
        // {
        //    AdvanceState();
        // }
        if (stateMachine.CurrentStateKey == GameStates.Combat)
        {
            if (GUI.Button(new Rect(x, y, width, height), "DEBUG INSTAKILL ENEMY"))
            {
                if (enemyShip.boss) {
                    var mainCabins = enemyShip.GetMainCabins();
                    foreach(var mc in mainCabins) {
                        mc.TakeDamage(1000);
                    }
                }
                else {
                    enemyShip.GetMainCabin().TakeDamage(1000);
                }
                //playerShip.GetMainCabin().TakeDamage(1000);
                return;
            }


            if (GUI.Button(new Rect(10, y -  height * 1.5f, width, height), "DEBUG INSTAKILL PLAYER")) {
                playerShip.GetMainCabin().TakeDamage(1000);
                //playerShip.GetMainCabin().TakeDamage(1000);
                return;
            }
        }

        /* if (stateMachine.CurrentStateKey == GameStates.GameOver)
         {
             if (GUI.Button(new Rect(500, 400, 150, 40), "RESTART"))
             {
                 GameManager.Instance.RestartGame();
             }
         }*/
    }

    // TODO: (if will ever do) map selection
}

