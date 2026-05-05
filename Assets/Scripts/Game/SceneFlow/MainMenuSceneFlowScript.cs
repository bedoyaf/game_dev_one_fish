using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


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
    private float startMenuX;

    [Tooltip("The parent object for all save slot selection items")]
    [SerializeField]
    private GameObject saveSelectionItemsParent;
    private float startSaveX;
    
    
    [SerializeField]
    private Image background;
    private float startBackX;


    public float move_offset = 100;


    public void Start()
    {
        startBackX = background.transform.position.x;
        startMenuX = mainMenuItemsParent.transform.position.x;
        startSaveX = saveSelectionItemsParent.transform.position.x;

        mainMenuItemsParent.SetActive(true);
        saveSelectionItemsParent.SetActive(false);

    }

    public void OnPlayButtonClicked()
    {
        // move the main menu items, the back and the saves
        background.transform.DOMoveX(startBackX - move_offset*0.5f, 0.2f);

        mainMenuItemsParent.transform.DOMoveX(startMenuX - move_offset, 0.2f).onComplete += (
            () => { 
                mainMenuItemsParent.SetActive(false);
                saveSelectionItemsParent.SetActive(true);

                saveSelectionItemsParent.transform.DOMoveX(startSaveX - move_offset, 0.2f);
            });

    }

    public void OnSettingsButtonClicked()
    {
        // TODO: show the settings scene etc.
    }

    public void OnBackFromSlotsClicked()
    {
        // move the main menu items, the back and the saves
        background.transform.DOMoveX(startBackX, 0.2f);

        saveSelectionItemsParent.transform.DOMoveX(startSaveX, 0.2f).onComplete += (
            () => {
                mainMenuItemsParent.SetActive(true);
                saveSelectionItemsParent.SetActive(false);

                mainMenuItemsParent.transform.DOMoveX(startMenuX, 0.2f);
            });

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
