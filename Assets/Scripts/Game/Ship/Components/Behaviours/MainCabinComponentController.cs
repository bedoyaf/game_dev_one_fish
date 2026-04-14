using UnityEngine;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
[RequireComponent(typeof(ShipComponentController))]
public class MainCabinComponentController : BehaviourComponentControllerAbstract
{
    public override void OnActivate()
    {
        shipComponentController.DeactivateComponent();
    }

    public override void OnDeactivate()
    {

    }

    public override void OnTargetSelected(TargetingData target)
    {

    }
}
