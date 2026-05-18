using DG.Tweening;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Main cabin, worth considering if it should have a battery and generator implementation
/// </summary>
public class MainCabinComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] private int hookDamage = 1;

    [SerializeField] private HookShotScript hookShot;

    
    // NOTE: - need to figure out, cooldown timing
    //       - what should happen if there's a shield on the target
    //       


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
        // ???
        return false;
    }

    public override bool OnTargetSelected(TargetingData target)
    {
        var targetMesh = target.target;

        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        
        shipComponentController.DeactivateComponent();

        if (targetShipComponent.transform.parent == transform.parent)
        {
            Debug.Log("Wrong ship");
            return false;
        }

        
        Vector3 exactTargetPosition = targetShipComponent.transform.position
            + targetShipComponent.transform.right * 0.5f + target.ComponentOffset
            + targetShipComponent.transform.forward * 0.5f;
        
        // TODO: uncommend once debugged
        // if(targetShipComponent.broken)
            ShootHookAtDamaged(targetShipComponent, exactTargetPosition);
        // else
        //    ShootHookAtWorking(targetShipComponent, exactTargetPosition);

        return true;
    }

    private void ShootHookAtWorking(ShipComponentController targetShipComponent, Vector3 targetPosition)
    {
        // SFX of hook shooting from this cabin
        hookShot.ShootHookAt(targetShipComponent, targetPosition,
            false,   // don't pull the component toward the main ship
            () => { DealDamage(targetShipComponent); },
            () => {}
        );
    }

    private void DealDamage(ShipComponentController targetShipComponent)
    {
        targetShipComponent.TakeDamage(hookDamage);
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

    // Called when the hook reaches the component and breaks it off (animation finished)
    private void BreakOffComponent(ShipComponentController targetShipComponent)
    {
        ShipController targetShip = targetShipComponent.shipController;

        // TODO: effect of flying away destroyed...
        var brokeOf = targetShip.componentGrid.GetAllSeparatedComponentsAfterRemoval(
            targetShipComponent.placementRules.connectedTile, false);
        // then kill them

        Debug.Log("Breaking off");

        // actually break this component off
        targetShip.componentGrid.RemoveComponent(targetShipComponent.placementRules.connectedTile, true, false);

        // tween the removed components positions
        foreach (var item in brokeOf)
        {
            // down position + slightly random left/right
            var target = item.transform.position.SetZ(-5f) 
                + 2f * (0.5f - Random.value) * Vector3.right;

            float randomAngle = Random.Range(0f, 10f);

            item.gameObject.transform.DOLocalRotate(
                new Vector3(0f, randomAngle, 0f),
                1f,
                RotateMode.LocalAxisAdd
            );

            item.gameObject.transform.DOMove(target, 2f).onComplete +=

                // then kill them...
                () => { Destroy(item.gameObject); };
        }


    }

    private void PickupComponent(ShipComponentController targetShipComponent)
    {
        GameManager.Instance.currentGameplayManager.combatController.AddComponentLoot(targetShipComponent);
    }

}
