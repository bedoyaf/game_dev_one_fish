using UnityEngine;
using System.IO;

/// <summary>
/// Current state:
///     (0-data) Global persistent data between application runs (Game Settings)
///     (1-data) Persistent data per save slot (each different / default if none given) 
///                 also saved between runs
///     (2-data) Run data (changes between each run)
///     
/// 
/// </summary>
public class GameManager : SmartSingleton<GameManager> {

    // Add to any scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        // Create the black scene transition instance
        var obj = new GameObject("GameManager[Persistent]");
        obj.AddComponent<GameManager>();
        DontDestroyOnLoad(obj);
    }


    private SceneTransitionScript sceneTransitionInstance;
    public void TransitionScene(string sceneName) {
        sceneTransitionInstance.LoadScene(sceneName);
    }

    // NOTE: redo this maybe 
    public void SetTransitionInstance(SceneTransitionScript instance) {
        sceneTransitionInstance = instance;
    }




    // NOTE: maybe overcomplicated
    // But I like the split of certain features between
    //                                      the GameManager (high level management - starting/killing the game) 
    //                                  and the Flow Manager (spawning enemies, events, handling this stuff)
    public GameplayFlowManager currentGameplayManager;
    public void SetGameplayFlowInstance(GameplayFlowManager instance) {
        currentGameplayManager = instance;
    }

    [SerializeField] private bool tutorialFinished;
    public bool TutorialFinished => tutorialFinished;


    public static bool IsPaused => MyTime.pausedOverride < 1;
    public bool IsInCombat =>
        // In Combat state and not dead
           currentGameplayManager.stateMachine.CurrentStateKey == GameplayFlowManager.GameStates.Combat
        && currentGameplayManager.PlayerShip.GetMainCabin().health > 0;

    public bool IsRepairing =>
           currentGameplayManager.stateMachine.CurrentStateKey == GameplayFlowManager.GameStates.Repairs;
    public SFXGameplayManager SFXManager => currentGameplayManager.sfx;

    /// <summary>
    /// Called when a Save Slot IS selected*
    /// 
    /// Instantiates the Gameplay Scene and begins the main Game loop.
    /// 
    /// * if in Editor Debug etc. if no slot is selected, a default one will be provided
    /// </summary>
    public void StartGame()
    {
        LoadRunFromJson();

        if (currentRunData == null)
        {
            StartNewRun();
        }

        TransitionScene("GameplayScene");
    }

    /// <summary>
    /// Call to change the current Run Data aka reset the Run.
    /// </summary>
    public void RestartGame()
    {
        MyTime.pausedOverride = 1;
        MyTime.slowDownOverride = 1;

        StartNewRun();

        TransitionScene("GameplayScene");
    }

    public void OnTutorialFinished() {
        tutorialFinished = true;
        RestartGame();
    }

    /// <summary>
    /// Shows a game menu with settings and the ability to leave the game.
    /// </summary>
    public void PauseGame() {
        // TODO: actually pause 
        // disable in-game interactions etc.
        AudioManager.Instance.StopAllSFX();
        MyTime.pausedOverride = 0;
        // maybe gameplay manager needs to know too
    }

    /// <summary>
    /// Called when the game should resume.
    /// </summary>
    public void ResumeGame() {
        // TODO: actually unpause
        // re-enable interactions etc.
        MyTime.pausedOverride = 1;

        // gameplay manager again ?

    }

    /// <summary>
    /// Ends the current run and opens the main menu again 
    /// </summary>
    public void ExitGame() {
        // TODO: save what needs to be saved
        // etc..
        MyTime.pausedOverride = 1;
        MyTime.slowDownOverride = 1;

        GameManager.Instance.SaveRunToJson();

        TransitionScene("MainMenuScene");
    }




    // TODO: actual GameManager interface to access the datas

    // -------------------------------------------------------


    // 0-data
    private GameSettings _gameSettingsInstance;

    public GameSettings GameSettingsInstance {
        get {

            // Default run-time only settings
            if (_gameSettingsInstance == null) {
                _gameSettingsInstance = ScriptableObject.CreateInstance<GameSettings>();
            }

            return _gameSettingsInstance;
        }

        set { _gameSettingsInstance = value; }
    }


    // -------------------------------------------------------

    // 1-data
    private SaveSlotData _activeSaveSlot;
    public SaveSlotData activeSaveSlot {
        get {
            // No save slot has been set
            // (ie. running in editor without picking
            if (_activeSaveSlot == null) {
                // Temporary run-time instance
                _activeSaveSlot = ScriptableObject.CreateInstance<SaveSlotData>();
            }

            return _activeSaveSlot;
        }
        set {
            _activeSaveSlot = value;
        }
    }

    public void SelectSaveSlot(SaveSlotData slot) {
        // Maybe some checks ?
        activeSaveSlot = slot;
    }

    public RunData CurrentRunData => currentRunData;

    /// <summary>
    /// Creates a new run and initializes RunData
    /// </summary>
    public void StartNewRun()
    {
        currentRunData = ScriptableObject.CreateInstance<RunData>();
        Debug.Log("Started new run");
    }

    /// <summary>
    /// Saves the current run to JSON
    /// </summary>
    public void SaveRunToJson()
    {
        if (currentRunData == null)
        {
            Debug.LogError("No active run to save");
            return;
        }

        // Capture current game state
        currentRunData.mapGraph = currentGameplayManager.mapController.graph;
        currentRunData.map_pos = currentGameplayManager.mapController.CurrentNode.id;
        currentRunData.playerShipState = currentGameplayManager.PlayerShip.SaveState();

        string savePath = GetSaveFilePath();
        currentRunData.SaveToJson(savePath);
    }

    /// <summary>
    /// Loads a run from JSON
    /// </summary>
    public void LoadRunFromJson()
    {
        string savePath = GetSaveFilePath();
        currentRunData = RunData.LoadFromJson(savePath);
    }

    /// <summary>
    /// Gets the save file path for the current save slot
    /// </summary>
    private string GetSaveFilePath()
    {
        string persistentPath = Application.persistentDataPath;
        return Path.Combine(persistentPath,$"slot_{activeSaveSlot.slotIndex}_run.json"
);
    }

    // -------------------------------------------------------

    // 2-data 
    private RunData currentRunData;
}
