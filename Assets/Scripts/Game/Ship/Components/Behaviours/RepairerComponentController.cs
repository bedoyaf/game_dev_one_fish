using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// spawns a shield on a ship component
/// Shield just spawns visualy, damage and collision is still handeled by shipcomponentcontroller
/// </summary>
public class RepairerComponentController : BehaviourComponentControllerAbstract
{
    public override bool CanClickOnNow => 
        cooldown.IsReady && 
        !shipComponentController.broken && 
        shipController.GetCurrency > 0 && 
        shipController.GetEnergy >= shipComponentController.requiredEnergy;

    public void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }
    public override bool OnActivate()
    {        
        Debug.Log("Selecting Repair Target");
        MouseController.Instance.EnterTargetingMode(this);
        return true;
    }

    public override void OnAgentActivate(TargetingData data)
    {
        OnTargetSelected(data);
    }

    public override bool OnDeactivate()
    {
        return true;
    }

    public override bool OnTargetSelected(TargetingData target)
    {
        ShipComponentMeshController targetMesh = target.target;
        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        var targetShip = targetShipComponent.shipController;
        if (targetShipComponent.transform.parent != transform.parent)
        {
            Debug.Log("Wrong ship");
            shipComponentController.DeactivateComponent();
            return false;
        }
        // Check that we can afford the repair and that the target is broken
        // In combat -> only broken components
        if (!targetShipComponent.broken)
        {
            Debug.Log("That component is not broken");
            shipComponentController.DeactivateComponent();
            return false;
        }

        // do the repair
        Debug.Log("Clicking Repairs");
        targetShipComponent.RepairClick();       
        shipComponentController.DeactivateComponent();
        if (cooldown != null) cooldown.Trigger();

        return true;
    }

    

    public override void ResetBehaviour()
    {
        
    }
}
