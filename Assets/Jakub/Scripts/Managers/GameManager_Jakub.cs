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
public class GameManager_Jakub : SmartSingleton<GameManager_Jakub>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void StartGame()
    {
        // Take the current save slot data

        // Start the game depending on it

        // Scene transition
    }



    // TODO: actual GameManager interface to access the datas

    // -------------------------------------------------------

    // 0-data
    private GameSettings GameSettingsInstance;


    // -------------------------------------------------------

    // 1-data
    private SaveSlotData _activeSaveSlot;
    private SaveSlotData ActiveSaveSlot
    {
        get
        {
            // No save slot has been set
            // (ie. running in editor without picking
            if (_activeSaveSlot == null)
            {
                // Temporary run-time instance
                _activeSaveSlot = ScriptableObject.CreateInstance<SaveSlotData>();
            }

            return _activeSaveSlot;
        }
        set
        {
            _activeSaveSlot = value;
        }
    }

    public void SelectSaveSlot(SaveSlotData slot)
    {
        // Maybe some checks ?
        ActiveSaveSlot = slot;
    }

    

    // -------------------------------------------------------

    // 2-data 

}
