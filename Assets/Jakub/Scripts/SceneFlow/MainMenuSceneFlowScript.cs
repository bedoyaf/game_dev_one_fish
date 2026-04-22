using UnityEngine;


/// <summary>
/// Home for all functionality relating to the Main Menu scene flow:
/// includes:
///     Play button - shows available save slots (WIP)
///         Save slots - when shown, can select one of the slots / delete them
///             Back Button - hides the save slots
///     Settings button - shows a separate settings scene
///         Back Button - hides the settings, shows the main menu again
///     Exit button - quits the application 
///     Credits (?) - TBD
/// </summary>
public class MainMenuSceneFlowScript : MonoBehaviour
{

    [Tooltip("The parent object for all main menu items")]
    [SerializeField]
    private GameObject mainMenuItemsParent;

    [Tooltip("The parent object for all save slot selection items")]
    [SerializeField]
    private GameObject saveSelectionItemsParent;

    public void Start()
    {
        mainMenuItemsParent.SetActive(true);
        saveSelectionItemsParent.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        mainMenuItemsParent.SetActive(false);
        saveSelectionItemsParent.SetActive(true);
    }

    public void OnSettingsButtonClicked()
    {
        // TODO: show the settings scene etc.
    }

    public void OnBackFromSlotsClicked()
    {
        mainMenuItemsParent.SetActive(true);
        saveSelectionItemsParent.SetActive(false);
    }



    public void OnExitButtonClicked()
    {
      
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


}
