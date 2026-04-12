using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera cam;
    private InputAction clickAction;

    public static MouseController Instance { get; private set; }

    private ClickMode currentMode = ClickMode.Default;
    private IShipComponentBehaviour activeComponent;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cam = Camera.main;

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        //clickAction.performed += OnClick;
    }


    void OnEnable() {
        clickAction.performed += OnClick;
        clickAction.Enable();
    }
    void OnDisable()
    {
        clickAction.performed -= OnClick;
        clickAction.Disable();
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
            return;

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        var target = hit.collider.GetComponentInParent<ShipComponentMeshController>();
        if (target == null)
            return;

        switch (currentMode)
        {
            case ClickMode.Default:
                HandleDefaultClick(target);
                break;

            case ClickMode.ComponentTargeting:
                HandleComponentTargetClick(target);
                break;
        }
    }

    private void HandleDefaultClick(ShipComponentMeshController comp)
    {
        Debug.Log("Default click: " + comp.name);
        comp.OnMouseClick();
    }

    private void HandleComponentTargetClick(ShipComponentMeshController target)
    {
        Debug.Log($"Targeting {target.name}");
        activeComponent.OnTargetSelected(target);/*.transform.parent.GetComponent<IShipComponentBehaviour>()*/

        ExitTargetingMode();
    }

    public void EnterTargetingMode(IShipComponentBehaviour component)
    {
        activeComponent = component;
        currentMode = ClickMode.ComponentTargeting;

        Debug.Log("Entered targeting mode");

       // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ExitTargetingMode()
    {
        activeComponent = null;
        currentMode = ClickMode.Default;

        Debug.Log("Exited targeting mode");

       // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public enum ClickMode
    {
        Default,
        ComponentTargeting
    }
}
