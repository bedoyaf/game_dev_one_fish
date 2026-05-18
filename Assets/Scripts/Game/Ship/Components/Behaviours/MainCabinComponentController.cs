using UnityEngine;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
public class MainCabinComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] private int hookDamage = 1;

    public override bool CanClickOnNow
    {
        get
        {
            if (shipController == null) Debug.LogError("shipController is NULL (cabin)");
            // TODO: add cooldonw here too
            return
            !shipComponentController.broken &&
            shipController.GetEnergy >= shipComponentController.requiredEnergy;
        }
    }

    public override bool OnActivate()
    {
        // TODO: cooldown (+ check currently held blocks maybe)
        // maybe allow shooting and ripping, but just don't insert to inventory

        MouseController.Instance.EnterTargetingMode(this);

        return true;
    }

    // NOTE: cannot be done by agent
    public override void OnAgentActivate(TargetingData data)
    {
        shipComponentController.DeactivateComponent();
    }

    public override bool OnDeactivate()
    {
        // No action on click -> false
        return false;
    }

    public override bool OnTargetSelected(TargetingData target)
    {
        var targetMesh = target.target;

        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        ShipController targetShip = targetShipComponent.shipController;

        shipComponentController.DeactivateComponent();

        if (targetShipComponent.transform.parent == transform.parent)
        {
            Debug.Log("Wrong ship");
            return false;
        }

        ShootHookAtDamaged(targetShipComponent);

        return true;
    }

    private void ShootHookAtWorking(ShipComponentController targetShipComponent)
    {
        // SFX of hook shooting from this cabin

        // when over, deal damage to that component
    }

    private void ShootHookAtDamaged(ShipComponentController targetShipComponent)
    {
        // SFX of hook shooting from this cabin

        // when over, seperate that component from the ship

        ShipController targetShip = targetShipComponent.shipController;
        
        // TODO: effect of flying away destroyed...
        var brokeOf = targetShip.componentGrid.GetAllSeparatedComponentsAfterRemoval(
            targetShipComponent.placementRules.connectedTile, false);
        // then kill them

        Debug.Log("Breaking off");

        // actually break this component off
        targetShip.componentGrid.RemoveComponent(targetShipComponent.placementRules.connectedTile, true, true);


    }

}
