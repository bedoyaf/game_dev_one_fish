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
            
            return cooldown.IsReady &&
            !shipComponentController.broken &&
            shipController.GetEnergy >= shipComponentController.requiredEnergy;
        }
    }

    public void Start()
    {
        if (shipController != null && !shipController.playerShip)
            hookShot.HideHook();

        // maybe set by parent ?
        cooldown = GetComponent<ComponentCooldown>();
    
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

        ShootHookAt(targetShipComponent, exactTargetPosition);
        if (cooldown != null) cooldown.Trigger();

        return true;
    }

    private void ShootHookAt(ShipComponentController targetShipComponent, Vector3 targetPosition)
    {
        // SFX of hook shooting from this cabin
        // when arrive at target, checks using a probe, if shield present
        hookShot.ShootHookAt(targetShipComponent, targetPosition,
            (bool hitTarget) => {
                // if the target is protected via shield -> just go back
                // if broken -> probe will not hit
                if (!hitTarget && !targetShipComponent.broken)
                    // don't pull the component toward the main ship
                    return false;

                // Uncomment once debugged:
                /*
                // else check if broken -> not = do damage
                if (!targetShipComponent.broken)
                {
                    DealDamage(targetShipComponent);
                    // don't pull the component toward the main ship
                    return false;
                }
                */

                BreakOffComponent(targetShipComponent);
                // else pull back 
                return true;

            },   
            (bool pulled) => { 
                if(pulled)
                    PickupComponent(targetShipComponent);
            }
        );

    }

    private void DealDamage(ShipComponentController targetShipComponent)
    {
        targetShipComponent.TakeDamage(hookDamage);
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

        // notify enemy now, so that cannot repair and activate on the way
        GameManager.Instance.currentGameplayManager.combatController.InformEnemyOfComponentRemoved();

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
