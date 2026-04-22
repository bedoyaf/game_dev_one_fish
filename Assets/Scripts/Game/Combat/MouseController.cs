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

    //Directional targeting
    private Vector3 currentDirection = Vector3.right;

    private Vector3[] directions = new Vector3[]
    {
    Vector3.up,
    Vector3.right,
    Vector3.down,
    //Vector3.left
    };

    private string[] directionNames = new string[] //switched left and right:(
    {
    "UP",
    "LEFT",
    "DOWN",
    //"RIGHT"
    };

    private int directionIndex = 0;

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

        activeComponent.OnTargetSelected(new TargetingData(target, currentDirection));

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



    //Directional shit

    private void Update()
    {
        // When scrolling -> cycle through attack direction
        float scroll = Mouse.current.scroll.ReadValue().y;

        if(scroll != 0)
        {
            CycleDirection((int) scroll);
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;

        if (GUI.Button(new Rect(10, 60, 120, 40), directionNames[directionIndex], style))
        {
            CycleDirection();
        }
    }

    private void CycleDirection(int dir=1)
    {
        directionIndex = (directions.Length + directionIndex + dir) % directions.Length;
        currentDirection = directions[directionIndex];

        Debug.Log("Direction set to: " + currentDirection);
    }
}
