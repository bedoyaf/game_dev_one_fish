using UnityEngine;
using UnityEngine.UIElements;

public class CabinHookIndicator : MonoBehaviour
{

    private GameplayFlowManager gameplayManager;
    private ShipComponentController shipComponent;

    [SerializeField]
    private MainCabinComponentController cabin;

    [SerializeField]
    private ComponentCooldown cooldown;

    [SerializeField]
    private Color unusableColor;
    [SerializeField]
    private Color defaultColor;

    [SerializeField]
    protected SpriteRenderer quadIndicator;

    private Vector3 originalScale;

    void Start()
    {
        gameplayManager = GameManager.Instance.currentGameplayManager;
        shipComponent = GetComponentInParent<ShipComponentController>();
        originalScale = quadIndicator.transform.localScale;
        defaultColor = quadIndicator.color;
    }

    // Update is called once per frame
    void Update()
    {

        if (cabin == null)
            return;

        if (shipComponent != null && shipComponent.broken)
        {
            quadIndicator.transform.localScale = Vector3.zero;
            return;
        }

        // Basically only show the indicators when in combat
        if (gameplayManager != null)
        {
            if (gameplayManager.InCombat)
            {
                quadIndicator.transform.localScale = originalScale;
            }
            else
            {
                quadIndicator.transform.localScale = Vector3.zero;
            }
        }

        bool clickable = cabin.CanClickOnNow;

        if (!cabin.shipComponentController.shipController.playerShip)
        {
            quadIndicator.transform.localScale = Vector3.zero;
            return;
        }

        if (cooldown == null)
        {
            quadIndicator.transform.localScale = clickable ? Vector3.one : Vector3.zero;
        }
        else
        {
            quadIndicator.color = cooldown.IsReady ? defaultColor : unusableColor;
        }
    }
}
