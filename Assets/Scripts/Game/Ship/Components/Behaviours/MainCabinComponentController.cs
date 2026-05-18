using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
public class MainCabinComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] private int hookDamage = 1;

    [SerializeField] private HookShotScript hookShot;

    // TODO: list of stored components from the current combat

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

    public void Start()
    {
        if (shipController != null && !shipController.playerShip)
            hookShot.HideHook();
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

        // check if the target is broken ->
        Vector3 exactTargetPosition = targetShipComponent.transform.position
            + targetShipComponent.transform.right * 0.5f + target.ComponentOffset
            + targetShipComponent.transform.forward * 0.5f;
        ShootHookAtDamaged(targetShipComponent, exactTargetPosition);

        
        // else do damage

        return true;
    }

    private void ShootHookAtWorking(ShipComponentController targetShipComponent, Vector3 targetPosition)
    {
        // SFX of hook shooting from this cabin

        // when over, deal damage to that component
    }

    private void ShootHookAtDamaged(ShipComponentController targetShipComponent, Vector3 targetPosition)
    {
        // SFX of hook shooting from this cabin
        hookShot.ShootHookAt(targetShipComponent, targetPosition,
            true,   // pull the component toward the main ship
            () => { BreakOffComponent(targetShipComponent); },
            () => { PickupComponent(targetShipComponent); }
        );

        
    }

    private void BreakOffComponent(ShipComponentController targetShipComponent)
    {
        // when over, separate that component from the ship

        ShipController targetShip = targetShipComponent.shipController;

        // TODO: effect of flying away destroyed...
        var brokeOf = targetShip.componentGrid.GetAllSeparatedComponentsAfterRemoval(
            targetShipComponent.placementRules.connectedTile, false);
        // then kill them

        Debug.Log("Breaking off");

        // actually break this component off
        targetShip.componentGrid.RemoveComponent(targetShipComponent.placementRules.connectedTile, true, false);

        

        // pull the selected component toward ship with the hook SFX   
    }

    private void PickupComponent(ShipComponentController targetShipComponent)
    {

        // Big todo: into the inventory
        GameManager.Instance.currentGameplayManager.combatController.AddComponentLoot(targetShipComponent);

    }

}
