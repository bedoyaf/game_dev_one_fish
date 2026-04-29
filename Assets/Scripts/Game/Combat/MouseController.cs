using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera cam;
    private InputAction clickAction;
    private InputAction cancelAction;

    public static MouseController Instance { get; private set; }

    private ClickMode currentMode = ClickMode.Default;
    private IShipComponentBehaviour activeComponent;

    //Directional targeting
    private Vector3 currentDirection = Vector3.right;

    public static Vector3[] DIRECTIONS = new Vector3[]
    {
    Vector3.up,
    Vector3.right,
    Vector3.down,
    //Vector3.left
    };

    public static Vector3[] ENEMY_DIRECTIONS = new Vector3[]
    {
    Vector3.up,
    Vector3.left,
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

    private int directionIndex = 1;

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
        cancelAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
        //clickAction.performed += OnClick;
    }


    void OnEnable() {
        clickAction.performed += OnClick;
        clickAction.Enable();

        cancelAction.performed += OnRightClick;
        cancelAction.Enable();
    }
    void OnDisable()
    {
        clickAction.performed -= OnClick;
        clickAction.Disable();

        cancelAction.performed -= OnRightClick;
        cancelAction.Disable();
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

        // Get exact grid coordinates (if someone needed it)
        //var position = hit.point - target.transform.position + target.transform.localPosition;
        //int x = (int)position.x;
        //int z = (int)position.z;

        // Calculate offset to correct component tile
        Vector3 componentOffset = hit.point - target.transform.parent.position;
        componentOffset = new Vector3((int)componentOffset.x, 0, (int)componentOffset.z);

        switch (currentMode)
        {
            case ClickMode.Default:
                HandleDefaultClick(target);
                break;

            case ClickMode.ComponentTargeting:
                HandleComponentTargetClick(target, componentOffset);
                break;

            case ClickMode.Repairing:
                HandleComponentRepairClick(target);
                break;
        }
    }

    public void EnterRepairsMode()
    {
        currentMode = ClickMode.Repairing;

        if(activeComponent != null)
        {
            activeComponent.ResetBehaviour();
            activeComponent = null;
        }

        // icon
        if (mouseSwitch != null)
            StopCoroutine(mouseSwitch);

        Cursor.SetCursor(repairIcon, Vector2.zero, CursorMode.Auto);
    }
    public void ExitRepairsMode()
    {
        currentMode = ClickMode.Default;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        // if targeting -> cancel
        if(currentMode == ClickMode.ComponentTargeting)
        {
            // NOT worth now, rework energy consumption on succesfull usage only if want this
        } 
    }

    private void HandleDefaultClick(ShipComponentMeshController comp)
    {
        Debug.Log("Default click: " + comp.name);
        comp.OnMouseClick();
    }

    private void HandleComponentTargetClick(ShipComponentMeshController target, Vector3 componentOffset)
    {
        Debug.Log($"Targeting {target.name}");

        activeComponent.OnTargetSelected(new TargetingData(target, currentDirection, componentOffset));

        ExitTargetingMode();
    }

    private void HandleComponentRepairClick(ShipComponentMeshController comp)
    {
        Debug.Log("Repair click: " + comp.name);
        comp.OnRepairClick();
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
        ComponentTargeting,
        Repairing
    }



    //Directional shit
    private Coroutine mouseSwitch = null;

    private void Update()
    {
        // When scrolling -> cycle through attack direction
        float scroll = Mouse.current.scroll.ReadValue().y;

        if(scroll != 0)
        {
            CycleDirection((int) scroll);

            if (mouseSwitch != null)
                StopCoroutine(mouseSwitch);
            mouseSwitch = StartCoroutine(nameof(ShowMouseIcon));
        }
    }


    public Texture2D upArrow;
    public Texture2D downArrow;
    public Texture2D rightArrow;
    public Texture2D repairIcon;


    IEnumerator ShowMouseIcon()
    {
        Texture2D[] dirTextures = {
            upArrow,    // "UP",
            rightArrow, // "LEFT",
            downArrow,  // "DOWN",
        };

        Cursor.SetCursor(dirTextures[directionIndex],
            new Vector2(dirTextures[directionIndex].width/2,
                        dirTextures[directionIndex].height/2), 
            CursorMode.Auto);

        yield return new WaitForSeconds(1f);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }


    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;

        if (GUI.Button(new Rect(10, 80, 120, 40), directionNames[directionIndex], style))
        {
            CycleDirection();
        }
    }

    private void CycleDirection(int dir=1)
    {
        directionIndex = (DIRECTIONS.Length + directionIndex + dir) % DIRECTIONS.Length;
        currentDirection = DIRECTIONS[directionIndex];
    }
}
