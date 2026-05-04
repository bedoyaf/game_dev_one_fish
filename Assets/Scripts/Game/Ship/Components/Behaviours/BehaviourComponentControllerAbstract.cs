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

    public void Awake()
    {
      //  shipController = GetComponentInParent<ShipController>(); componentcontroller sets it for him:)
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
    /// </summary>
    public abstract void OnActivate();

    public abstract void OnAgentActivate(TargetingData target); //TODO might be worth considering implementing default function that just deactivates

    /// <summary>
    /// component deactivation
    /// </summary>
    public abstract void OnDeactivate();
    /// <summary>
    /// On targeting of another component, for components that use the mouse script and need another component to do its job
    /// </summary>
    public abstract void OnTargetSelected(TargetingData target);

    public virtual void ResetBehaviour()
    {

    }
}
