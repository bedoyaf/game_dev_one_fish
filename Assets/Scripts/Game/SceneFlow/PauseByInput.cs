using UnityEngine;
using UnityEngine.InputSystem;

public class PauseByInput : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseAction;
    private GameUIScript gameui;
    private void OnEnable()
    {
        gameui = GetComponent<GameUIScript>();
        pauseAction.action.performed += OnPause;
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPause;
        pauseAction.action.Disable();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        gameui.OnPauseClicked();
    }
}
