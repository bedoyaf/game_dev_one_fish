using UnityEngine;


/// <summary>
/// Home for all functionality relating to the Gameplay scene flow:
/// includes:
///     Pause button (TEMP?) - pauses the game
///         Resume   - resumes the current game
///         Restart  - restarts the run (losing progress)
///         Exit     - exits to the main menu
/// </summary>
public class GameUIScript : MonoBehaviour
{
    [Tooltip("Parent object for the pause menu items")]
    [SerializeField]
    private GameObject pauseMenuItemsParent;

    [SerializeField]
    private CombatController combatManager;

    void Start()
    {
        pauseMenuItemsParent.SetActive(false);    
    }


    public void OnPauseClicked()
    {
        pauseMenuItemsParent.SetActive(true);

        GameManager.Instance.PauseGame();
        combatManager.StopGame();
    }

    public void OnResumeClicked()
    {
        pauseMenuItemsParent.SetActive(false);

        GameManager.Instance.ResumeGame();
        combatManager.ResumeGame();
    }

    public void OnRestartClicked()
    {
        // NOTE: maybe not needed
        pauseMenuItemsParent.SetActive(false);

        GameManager.Instance.RestartGame();
    }

    public void OnExitGameClicked()
    {
        GameManager.Instance.ExitGame();
    }
}
