using UnityEngine;

/// <summary>
/// Father of indicator classes
/// Has build in cooldown handling and other useful info.
/// On it's own only does that it hides the indicator when not in combat
/// </summary>
public class IndicatorScript : MonoBehaviour
{
    private GameplayFlowManager gameplayManager;
    private ShipComponentController shipComponent;

    [SerializeField]
    protected MeshRenderer quadIndicator;

    [SerializeField]
    protected Material unusableQuadMaterial;
    protected Color unusableColor;
    protected Color defaultColor;

    protected Vector3 originalScale;

    // About cooldown
    protected bool lastFrameActive = true; // This way, it will run once at the start of the combat


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameplayManager = GameManager.Instance.currentGameplayManager;
        shipComponent = GetComponentInParent<ShipComponentController>();
        originalScale = quadIndicator.transform.localScale;
        defaultColor = quadIndicator.material.GetColor("_MainColor");

        if (unusableQuadMaterial != null) {
            unusableColor = unusableQuadMaterial.GetColor("_MainColor");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shipComponent != null && shipComponent.broken) {
            quadIndicator.transform.localScale = Vector3.zero;
            return;
        }

        // Basically only show the indicators when in combat
        if (gameplayManager != null) {
            if (gameplayManager.InCombat) {
                quadIndicator.transform.localScale = originalScale;
                OnUpdate();
            }
            else {
                quadIndicator.transform.localScale = Vector3.zero;
            }
        }
        else {
            OnUpdate();
        }
    }

    /// <summary>
    /// Sets color and degrees based on cooldown.
    /// Yes, for nice OOP there should be another class with cooldown capabilities, but I don't care
    /// </summary>
    public void CooldownUpdate(ComponentCooldown cooldown, bool clickable) {
        if (cooldown == null) {
            quadIndicator.transform.localScale = clickable ? Vector3.one : Vector3.zero;
        }
        else {
            quadIndicator.material.SetFloat("_Degrees", cooldown.RemainingInDegrees);
            if (lastFrameActive != clickable) {
                lastFrameActive = !lastFrameActive;
                quadIndicator.material.SetColor("_MainColor", lastFrameActive ? defaultColor : unusableColor);
            }
        }
    }

    // Update for chilren
    public virtual void OnUpdate() { }
}
