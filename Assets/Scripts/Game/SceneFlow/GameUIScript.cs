using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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

    [SerializeField]
    private GameplayFlowManager gameplayFlowManager;

    [SerializeField]
    private Button skipButton;

    [SerializeField]
    private Button endRepairsButton;

    [SerializeField]
    private GameObject GameOverMenu;

    [SerializeField]
    private TextMeshProUGUI EnergyLabel;

    [SerializeField]
    private TextMeshProUGUI ScrapLabel;

    [SerializeField]
    private ShipController playerShip;

    [SerializeField] private TutorialController tutorialController;

    private void Awake()
    {
        playerShip.onEnergyChanged.AddListener(UpdateCombatUI);
    }
    void Start()
    {
        skipButton.gameObject.SetActive(false);
        endRepairsButton.gameObject.SetActive(false);
        pauseMenuItemsParent.SetActive(false);   
        UpdateCombatUI();
    }


    private void Update()
    {
      //  UpdateCombatUI();
    }

    public void UpdateCombatUI()
    {
      //  Debug.Log("changing energy");
        EnergyLabel.text = playerShip.storedEnergy.ToString()+"/"+ playerShip.batteryCapacity.ToString();
        ScrapLabel.text = playerShip.storedMoney.ToString();
    }

    public void OnPauseClicked()
    {
        pauseMenuItemsParent.SetActive(true);


        if (tutorialController.IsRunning) tutorialController.PauseTutorial();

        GameManager.Instance.PauseGame();
        combatManager.StopGame();
    }

    public void OnResumeClicked()
    {
        pauseMenuItemsParent.SetActive(false);

        if (tutorialController.IsRunning) tutorialController.ResumeTutorial();

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


    // -------------------------------------------------

    public void ShowSkipButton()
    {
        skipButton.gameObject.SetActive(true);
    }

    public void HideSkipButton()
    {
        skipButton.gameObject.SetActive(false);
    }

    public void OnBuildCancel()
    {
        gameplayFlowManager.StopModifying();
    }

    // --------------------------------------------------

    public void ShowRepairsButton()
    {
        endRepairsButton.gameObject.SetActive(true);
    }

    public void HideRepairsButton()
    {
        endRepairsButton.gameObject.SetActive(false);
    }

    public void OnRepairsCancel()
    {
        gameplayFlowManager.ExitRepairsMode();
    }

    public void ShowGameOver()
    {
        GameOverMenu.gameObject.SetActive(true);
    }

    public void SkipTutorial()
    {
        gameplayFlowManager.SkipTutorial();
    }
    public void HideGameOver()
    {
        GameManager.Instance.RestartGame();
        GameOverMenu.gameObject.SetActive(false);
    }

}
