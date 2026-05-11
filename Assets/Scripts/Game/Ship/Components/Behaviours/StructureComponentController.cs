using UnityEngine;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
public class StructureComponentController : BehaviourComponentControllerAbstract
{
    public override bool OnActivate()
    {
        return shipComponentController.DeactivateComponent();
    }

    public override void OnAgentActivate(TargetingData data)
    {
        shipComponentController.DeactivateComponent();
    }

    public override bool OnDeactivate() => false;
    

    public override bool OnTargetSelected(TargetingData target)
    {
        return false;
    }
}
