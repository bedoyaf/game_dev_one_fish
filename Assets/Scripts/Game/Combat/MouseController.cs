using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    private Camera cam;
    private InputAction clickAction;
    private InputAction cancelAction;

    public static MouseController Instance { get; private set; }

    private ClickMode currentMode = ClickMode.Default;
    private ClickMode previousMode = ClickMode.Default;
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


    //hover filip schizo code
    private ShipComponentMeshController hoveredComponent;
    [SerializeField] private RepairCostTooltip repaireTooltip;
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


    void OnEnable()
    {
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

    public void Reset()
    {
        ExitTargetingMode();
    }


    public ShipComponentMeshController GetMouseOver(ref RaycastHit hitInfo)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
            return null;

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        hitInfo = hit;
        // NOTE: might be null !!
        return hit.collider.GetComponentInParent<ShipComponentMeshController>();
    }

    public ComponentBuildingDrag GetMouseOverDraggable(ref RaycastHit hitInfo)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
            return null;

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        hitInfo = hit;
        // NOTE: might be null !!
        return hit.collider.GetComponentInParent<ComponentBuildingDrag>();
    }


    private void OnClick(InputAction.CallbackContext ctx)
    {
        // Ignore when the game is paused
        if (GameManager.IsPaused)
            return;

        // Ignore if not in combat / repairing
        if (!GameManager.Instance.IsInCombat && !GameManager.Instance.IsRepairing)
            return;


        RaycastHit hit = new();
        var target = GetMouseOver(ref hit);
        if (target == null)
            return;

        // Get exact grid coordinates (if someone needed it)
        //var position = hit.point - target.transform.position + target.transform.localPosition;
        //int x = (int)position.x;
        //int z = (int)position.z;

        // Calculate offset to correct component tile
        Vector3 componentOffset = hit.point - target.transform.parent.position;
        componentOffset = new Vector3((int)componentOffset.x, 0, (int)componentOffset.z);

        //AudioManager.Instance.PlaySFX(componentClick, hit.point);

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

        if (activeComponent != null)
        {
            activeComponent.ResetBehaviour();
            activeComponent = null;
        }

    }
    public void ExitRepairsMode()
    {
        currentMode = ClickMode.Default;
    }

    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        // if targeting -> cancel
        if (currentMode == ClickMode.ComponentTargeting)
        {
            // NOT worth now, rework energy consumption on succesfull usage only if want this
        }
    }

    private void HandleDefaultClick(ShipComponentMeshController comp)
    {
        //       Debug.Log("Default click: " + comp.name);
        var clickResult = comp.OnMouseClick();

        if (!clickResult) {
            ShowShortTermMouseIcon(ShortTermMouseEvent.FAIL, 0.1f);
            AudioManager.Instance.PlaySFX(failSound);
        }
    }

    private void HandleComponentTargetClick(ShipComponentMeshController target, Vector3 componentOffset)
    {
        //      Debug.Log($"Targeting {target.name}");

        var clickResult = activeComponent.OnTargetSelected(new TargetingData(target, currentDirection, componentOffset));

        ExitTargetingMode(clickResult);
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
        MyTime.slowDownOverride = 0.3f;

        Debug.Log("Entered targeting mode");

        // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ExitTargetingMode(bool clickResult=true)
    {
        activeComponent = null;
        currentMode = ClickMode.Default;
        MyTime.slowDownOverride = 1f;

      //  Debug.Log("Exited targeting mode");

        // TODO: maybe fail sound ?
        if (!clickResult) {
            ShowShortTermMouseIcon(ShortTermMouseEvent.FAIL, 0.3f);
            AudioManager.Instance.PlaySFX(failSound);
        }

        // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public enum ClickMode
    {
        Default,
        ComponentTargeting,
        Repairing
    }


    //Directional shit

    private void Update()
    {
        // No icon changes when paused
        if (GameManager.IsPaused)
            return;

        if (Mouse.current == null)
            return;

        // When scrolling -> cycle through attack direction
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            if (Mathf.Abs(scroll) > 0.1f)
            {
                CycleDirection(scroll > 0 ? 1 : -1);
                ShowShortTermMouseIcon(ShortTermMouseEvent.ROTATE, 0.1f);
            }
        }

        UpdateIcons(scroll);


        //hover schizo code
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit))
        {
            var comp = hit.collider.GetComponent<ShipComponentMeshController>();

            if (comp != hoveredComponent)
            {
                hoveredComponent = comp;

                if (comp != null &&
                    currentMode == ClickMode.Repairing &&
                    comp.OnHover().CanRepairThisComponent)
                {
                    repaireTooltip.Show(comp.OnHover().repairCost);
                }
                else
                {
                    repaireTooltip.Hide();
                }
            }
        }
        else
        {
            hoveredComponent = null;
            repaireTooltip.Hide();
        }

    }





    void OnDestroy()
    {
        // Reset the cursor, just in case
        if (shortTermMouseIconChange != null)
            StopCoroutine(shortTermMouseIconChange);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }




    public Texture2D upArrow;
    public Texture2D downArrow;
    public Texture2D rightArrow;
    public Texture2D failIcon;

    public Texture2D repairIcon;
    public Texture2D repairHighlightIcon;
    public Texture2D defaultMouseIcon;
    public Texture2D highlightMouseIcon;
    public Texture2D shieldIcon;
    public Texture2D rocketUpIcon;
    public Texture2D rocketDownIcon;
    public Texture2D rocketRightIcon;

    public SoundData failSound;

    /*
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;

        if (GUI.Button(new Rect(10, 80, 120, 40), directionNames[directionIndex], style))
        {
            CycleDirection();
        }
    }
    */
    private void CycleDirection(int dir = 1)
    {
        directionIndex = (DIRECTIONS.Length + directionIndex + dir) % DIRECTIONS.Length;
        currentDirection = DIRECTIONS[directionIndex];
    }



    // Mouse Icons

    // Long-term states:
    // Default   : when in normal mode and not-highlighting
    // Highlight : when in normal mode and over a 'clickable' component

    // Repair_Default : when in repair mode and not over a repair-able component
    // Repair_Highlight : when in repair mode and over a repair-able component 

    // Shield    : when in targeting mode with a shield
    // Rocket (direction) : when in targeting mode with a rocket (with the apropriate direction)

    // Short-term states:
    // Fail      : when an action fails
    // Direction : when cycling directions with mouse wheel



    // Observer pattern on the mouse icons
    void UpdateIcons(float scroll)
    {
        // If handling a short-term state (via Coroutine)
        if (shortTermMouseIconChange != null)
            return;

        RaycastHit hit = new();
        var target = GetMouseOver(ref hit);

        // only if no-short term override
        switch (currentMode)
        {
            case ClickMode.Default:

                // Highlight
                // Try to get the BehaviourComponent on the parent object
                // NOTE: not very sleek I guess :(
                if (target != null &&
                    target.BelongsToPlayer && 
                    target.gameObject.transform.parent.gameObject.TryGetComponent<BehaviourComponentControllerAbstract>(out var comp) &&
                    comp.CanClickOnNow)
                {
                    Cursor.SetCursor(highlightMouseIcon, Vector2.zero, CursorMode.Auto);
                }
                else
                {
                    // Default
                    // (only outside editor)
#if !UNITY_EDITOR
                    Cursor.SetCursor(defaultMouseIcon, Vector2.zero, CursorMode.Auto);
#endif
                    // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }

                // End highlight
                if (highlightedComponents.Count > 0) {
                    foreach(var highlight in highlightedComponents) {
                        highlight.RemoveHighlight();
                    }
                    highlightedComponents.Clear();
                }
                break;

            case ClickMode.ComponentTargeting:

                if (activeComponent != null)
                {
                    if (activeComponent is MissileComponentController)
                    {

                        Texture2D[] dirTextures = {
                            rocketUpIcon,    // "UP",
                            rocketRightIcon, // "LEFT",
                            rocketDownIcon,  // "DOWN",
                        };

                        var chosen = dirTextures[directionIndex];
                        Cursor.SetCursor(chosen, new Vector2(chosen.width * 0.5f, chosen.height * 0.5f), CursorMode.Auto);

                        HighlightAttack(rocketHighlightColor);
                    }
                    else if (activeComponent is ShieldComponentController)
                    {
                        Cursor.SetCursor(shieldIcon, new Vector2(shieldIcon.width * 0.5f, shieldIcon.height * 0.5f), CursorMode.Auto);
                        HighlightShields();
                    }

                    else if (activeComponent is RepairerComponentController) {
                        HighlightRepair(true);
                    }

                    else if (activeComponent is MainCabinComponentController) {
                        HighlightHook();
                    }
                }

                break;

            case ClickMode.Repairing:

                if (target != null &&
                    target.BelongsToPlayer &&
                    target.gameObject.transform.parent.gameObject.TryGetComponent<BehaviourComponentControllerAbstract>(out var repairTarget) &&
                    repairTarget.CanRepairNow)
                {
                    // Highlight
                    Cursor.SetCursor(repairHighlightIcon, Vector2.zero, CursorMode.Auto);
                }
                else
                {
                    // Default                    
                    Cursor.SetCursor(repairIcon, Vector2.zero, CursorMode.Auto);
                }

                HighlightRepair(false);
                break;
        }

        previousMode = currentMode;
    }


    public Material highlightMaterial;
    public float outlineWidth = 1.2f;
    public float fadeTime = 0.2f;
    private List<ShipComponentController> highlightedComponents = new();
    public Color rocketHighlightColor = new Color(255, 0, 3);
    public Color shieldHighlightColor = new Color(0, 16, 166);
    public Color repairHighlightColor = new Color(255, 204, 0);
    public Color hookAttackHighlightColor = new Color(255, 0, 3);
    public Color hookStealHighlightColor = new Color(38, 255, 0);


    private void HighlightAttack(Color color) {
        var enemyShip = GameManager.Instance.currentGameplayManager.EnemyShip;
        var newHighlighted = enemyShip.componentGrid.GetAllNonBrokenComponents();

        if (highlightedComponents.Count == 0) {
            HighlightComponents(newHighlighted, color);
        }
        else if (currentMode == previousMode) {
            CalculateDiffBetweenLists(highlightedComponents, newHighlighted, out var added, out var removed);
            HandleChangesInHighlight(added, removed, color);
        }

        highlightedComponents = newHighlighted;
    }
    private void HighlightShields() {
        var playerShip = GameManager.Instance.currentGameplayManager.PlayerShip;
        var newHighlighted = playerShip.componentGrid.GetAllNonBrokenComponents();

        if (highlightedComponents.Count == 0) {
            HighlightComponents(newHighlighted, shieldHighlightColor);
        }
        else if (currentMode == previousMode) {
            CalculateDiffBetweenLists(highlightedComponents, newHighlighted, out var added, out var removed);
            HandleChangesInHighlight(added, removed, shieldHighlightColor);
        }

        highlightedComponents = newHighlighted;
    }
    private void HighlightRepair(bool component) {
        List<ShipComponentController> newHighlighted;
        var playerShip = GameManager.Instance.currentGameplayManager.PlayerShip;

        if (component) {
            newHighlighted = playerShip.componentGrid.GetAllComponents().Where(x => x.IsBroken).ToList();
        }
        else {
            newHighlighted = playerShip.componentGrid.GetAllComponents().Where(x => x.CanRepairThisComponent).ToList();
        }

        if (highlightedComponents.Count == 0) {
            HighlightComponents(newHighlighted, repairHighlightColor);
        }
        else {
            CalculateDiffBetweenLists(highlightedComponents, newHighlighted, out var added, out var removed);
            HandleChangesInHighlight(added, removed, repairHighlightColor);
        }

        highlightedComponents = newHighlighted;
    }

    // Hook is bit more complicated than others
    private void HighlightHook() {
        var enemyShip = GameManager.Instance.currentGameplayManager.EnemyShip;
        var brokenComponents = enemyShip.componentGrid.GetAllBrokenComponents();

        if (highlightedComponents.Count == 0) {
            HighlightAttack(hookAttackHighlightColor);
            HighlightComponents(brokenComponents, hookStealHighlightColor);
            highlightedComponents.AddRange(brokenComponents);
        }
        else {
            var targets = enemyShip.componentGrid.GetAllNonBrokenComponents();
            var together = new List<ShipComponentController>();
            together.AddRange(targets);
            together.AddRange(brokenComponents);

            CalculateDiffBetweenLists(highlightedComponents, together, out var added, out var removed);
            HandleChangesInHighlight(added, removed, hookAttackHighlightColor);
            foreach (var target in targets) {
                target.ChangeHighlightColor(hookAttackHighlightColor);
            }
            foreach (var broken in brokenComponents) {
                broken.ChangeHighlightColor(hookStealHighlightColor);
            }
        }
    }

    private void HighlightComponents(List<ShipComponentController> highlightedComponents, Color color) {
        foreach (var highlight in highlightedComponents) {
            highlight.Highlight(highlightMaterial, color, outlineWidth, fadeTime);
        }
    }

    /// <summary>
    /// Calculates difference in members between the original list and new list.
    /// </summary>
    private void CalculateDiffBetweenLists<T>(List<T> original, List<T> newValues, out List<T> added, out List<T> removed) {
        added = new List<T>();
        removed = new List<T>(original);

        foreach (var item in newValues) {
            if (!removed.Remove(item)) {
                added.Add(item);
            }
        }
    }

    private void HandleChangesInHighlight(List<ShipComponentController> added, List<ShipComponentController> removed, Color componentColor) {
        HighlightComponents(added, componentColor);

        foreach (var highlight in removed) {
            highlight.RemoveHighlight();
        }
    }




    private enum ShortTermMouseEvent
    {
        ROTATE, FAIL
    }

    private Coroutine shortTermMouseIconChange = null;
    private void ShowShortTermMouseIcon(ShortTermMouseEvent eventType, float duration)
    {
        if (shortTermMouseIconChange != null)
            StopCoroutine(shortTermMouseIconChange);
        shortTermMouseIconChange = StartCoroutine(ShowMouseIcon(eventType, duration));
    }

    IEnumerator ShowMouseIcon(ShortTermMouseEvent eventType, float duration)
    {
        // ROTATE event
        if (eventType == ShortTermMouseEvent.ROTATE)
        {

            Texture2D[] dirTextures = {
                upArrow,    // "UP",
                rightArrow, // "LEFT",
                downArrow,  // "DOWN",
            };

            Cursor.SetCursor(dirTextures[directionIndex],
                new Vector2(dirTextures[directionIndex].width / 2,
                            dirTextures[directionIndex].height / 2),
                CursorMode.Auto);

            yield return MyTime.WaitForSeconds(duration);

        }
        // FAIL event
        else
        {
            Cursor.SetCursor(failIcon, new Vector2(failIcon.width * 0.5f, failIcon.height * 0.5f), CursorMode.Auto);

            yield return MyTime.WaitForSeconds(duration);

        }

        // Stop
        shortTermMouseIconChange = null;
    }

}
