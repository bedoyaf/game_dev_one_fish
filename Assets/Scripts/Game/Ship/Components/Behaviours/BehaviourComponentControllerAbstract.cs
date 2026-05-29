using UnityEngine;

/// <summary>
/// Abstract class for different component behaviours such as missiles, generators and more
/// </summary>
[RequireComponent(typeof(ShipComponentController))]
public abstract class BehaviourComponentControllerAbstract : MonoBehaviour, IShipComponentBehaviour
{
    protected ShipController shipController;
    public ShipComponentController shipComponentController { get; private set; }

    protected ComponentCooldown cooldown;
    public virtual bool CanClickOnNow { get; private set; } = false;

    public bool CanRepairNow => shipComponentController.CanRepairThisComponent;

    public void Awake()
    {
        //shipController = GetComponentInParent<ShipController>(); // componentcontroller sets it for him:)
        cooldown = GetComponent<ComponentCooldown>();
        shipComponentController = GetComponent<ShipComponentController>();
    }

    /// <summary>
    /// Called from editor, because otherwise this component just creates thousands of error messages
    /// </summary>
    public void SetShipController(ShipController shipController) {
        this.shipController = shipController;
    }

    /// <summary>
    /// Component activation
    /// 
    /// Return true, if successful, false if not
    /// </summary>
    public abstract bool OnActivate();

    public abstract void OnAgentActivate(TargetingData target); //TODO might be worth considering implementing default function that just deactivates

    /// <summary>
    /// component deactivation
    /// 
    /// Return true if successful, false if not
    /// </summary>
    public abstract bool OnDeactivate();
    /// <summary>
    /// On targeting of another component, for components that use the mouse script and need another component to do its job
    /// 
    /// Return true, if successful, false if not
    /// </summary>
    public abstract bool OnTargetSelected(TargetingData target);

    public virtual void ResetBehaviour()
    {

    }

    /// <summary>
    /// Optional hook for behaviours that enter targeting mode (aiming).
    /// Called when the targeting should be cancelled (for example when player presses the component again
    /// or the component breaks). Default implementation does nothing and returns false.
    /// </summary>
    public virtual bool CancelTargeting()
    {
        return false;
    }
}
