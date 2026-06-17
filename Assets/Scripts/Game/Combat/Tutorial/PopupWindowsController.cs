using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupWindowsController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Text component")]
    [SerializeField] private Image tutorialNote;

    [Tooltip("List of texts")]
    [SerializeField] private Sprite[] textPages;

    [Header("Input settings")]
    [Tooltip("Advance")]
    [SerializeField] private InputActionReference nextAction;

    [Header("Events")]
    [Tooltip("On sequence completed")]
    public UnityEvent OnSequenceCompleted;

    [SerializeField] private SoundData turnPageSound;
    [SerializeField] private bool playSoundOnStart;
    [SerializeField] private bool enterShip; // Yes, this should not be here.

    private int currentPageIndex = 0;

    private void OnEnable()
    {
        currentPageIndex = 0;
        UpdateTextDisplay();

        if (nextAction != null && nextAction.action != null)
        {
            nextAction.action.Enable();
            nextAction.action.performed += AdvanceText;
        }

        if (playSoundOnStart) {
            AudioManager.Instance.PlaySFX(turnPageSound);
        }

        if (enterShip) {
            GameManager.Instance.SFXManager.EnterEnemyShip();
        }
    }

    private void OnDisable()
    {
        if (nextAction != null && nextAction.action != null)
        {
            nextAction.action.performed -= AdvanceText;
        }
    }

    private void AdvanceText(InputAction.CallbackContext context)
    {
        currentPageIndex++;

        if (currentPageIndex >= textPages.Length)
        {
            CompleteSequence();
        }
        else
        {
            AudioManager.Instance.PlaySFX(turnPageSound);
            UpdateTextDisplay();
        }
    }

    private void UpdateTextDisplay()
    {

        if (textPages.Length > 0 && tutorialNote != null)
        {
            tutorialNote.sprite = textPages[currentPageIndex];
        }
    }

    private void CompleteSequence()
    {
        OnSequenceCompleted?.Invoke();
    }
}