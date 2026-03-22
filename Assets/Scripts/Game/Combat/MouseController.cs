using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera cam;
    private InputAction clickAction;

    void Awake()
    {
        cam = Camera.main;

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        clickAction.performed += OnClick;
    }

    void OnEnable()
    {
        clickAction.Enable();
    }

    void OnDisable()
    {
        clickAction.Disable();
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var comp = hit.collider.GetComponent<ShipComponentMeshController>();

            if (comp != null)
            {
                Debug.Log("Clicked: " + comp.name);
                comp.OnMouseClick();
            }
        }
    }
}
