using UnityEngine;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
public class StructureComponentController : BehaviourComponentControllerAbstract
{
    public override void OnActivate()
    {
        shipComponentController.DeactivateComponent();
    }

    public override void OnAgentActivate(TargetingData data)
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
