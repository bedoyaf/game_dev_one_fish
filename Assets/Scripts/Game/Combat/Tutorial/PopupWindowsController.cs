using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PopupWindowsController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Text component")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Tooltip("List of texts")]
    [TextArea(3, 5)]
    [SerializeField] private string[] textPages;

    [Header("Input settings")]
    [Tooltip("Advance")]
    [SerializeField] private InputActionReference nextAction;

    [Header("Events")]
    [Tooltip("On sequence completed")]
    public UnityEvent OnSequenceCompleted;

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
            UpdateTextDisplay();
        }
    }

    private void UpdateTextDisplay()
    {
        if (textPages.Length > 0 && textComponent != null)
        {
            textComponent.text = textPages[currentPageIndex];
        }
    }

    private void CompleteSequence()
    {
        OnSequenceCompleted?.Invoke();
    }
}