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

    [Tooltip("The parent object for all save slot selection items")]
    [SerializeField]
    private GameObject saveSelectionItemsParent;
    
    
    [SerializeField]
    private Transform background;
    private float startBackX;


    public float move_offset = 100;


    public void Start()
    {
        startBackX = background.position.x;

        mainMenuItemsParent.SetActive(true);
        saveSelectionItemsParent.SetActive(false);

    }

    public void OnPlayButtonClicked()
    {
        mainMenuItemsParent.SetActive(false);
        background.GetComponent<RectTransform>().DOAnchorPos3DX(startBackX - move_offset, 0.3f).onComplete += (
            () => { 
                saveSelectionItemsParent.SetActive(true);
            });

    }

    // Probably not needed 
    public void OnSettingsButtonClicked()
    {
        // TODO: show the settings scene etc.
    }

    public void OnBackFromSlotsClicked()
    {
        saveSelectionItemsParent.SetActive(false);
        mainMenuItemsParent.SetActive(true);
        background.GetComponent<RectTransform>().DOAnchorPos3DX(startBackX, 0.2f).onComplete += (
            () => {
                mainMenuItemsParent.SetActive(true);
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
