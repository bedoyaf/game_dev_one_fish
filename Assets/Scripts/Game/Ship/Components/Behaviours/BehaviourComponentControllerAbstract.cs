using UnityEngine;

/// <summary>
/// Abstract class for different component behaviours such as missiles, generators and more
/// </summary>
public abstract class BehaviourComponentControllerAbstract : MonoBehaviour, IShipComponentBehaviour
{
    protected ShipController shipController;
    protected ShipComponentController shipComponentController;

    public void Awake()
    {
        shipController = GetComponentInParent<ShipController>();
        shipComponentController = GetComponent<ShipComponentController>();
    }
    /// <summary>
    /// Component activation
    /// </summary>
    public abstract void OnActivate();
    /// <summary>
    /// component deactivation
    /// </summary>
    public abstract void OnDeactivate();
    /// <summary>
    /// On targeting of another component, for components that use the mouse script and need another component to do its job
    /// </summary>
    public abstract void OnTargetSelected(TargetingData target);
}
