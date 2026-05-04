using UnityEngine;

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




    public static bool IsPaused => MyTime.pausedOverride < 1;
    public bool IsInCombat => currentGameplayManager.stateMachine.CurrentStateKey == GameplayFlowManager.GameStates.Combat;

    /// <summary>
    /// Called when a Save Slot IS selected*
    /// 
    /// Instantiates the Gameplay Scene and begins the main Game loop.
    /// 
    /// * if in Editor Debug etc. if no slot is selected, a default one will be provided
    /// </summary>
    public void StartGame() {
        // Take the current save slot data (TODO)
        // TODO: play cutscene maybe if first time

        // Start the game depending on it
        // TODO: whatever needs to be done

        // Scene transition
        // NOTE: is it okay to do like this ?
        TransitionScene("GameplayScene");
    }

    /// <summary>
    /// Call to change the current Run Data aka reset the Run.
    /// </summary>
    public void RestartGame() {

        MyTime.pausedOverride = 1;
        // Maybe like this ?
        TransitionScene("GameplayScene");
    }

    /// <summary>
    /// Shows a game menu with settings and the ability to leave the game.
    /// </summary>
    public void PauseGame() {
        // TODO: actually pause 
        // disable in-game interactions etc.

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
    private SaveSlotData activeSaveSlot {
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



    // -------------------------------------------------------

    // 2-data 
    private RunData currentRunData;
}
