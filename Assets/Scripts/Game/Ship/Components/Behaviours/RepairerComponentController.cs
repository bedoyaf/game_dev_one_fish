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
    public bool CanClick => 
        cooldown.IsReady && 
        !shipComponentController.broken && 
        shipController.GetCurrency > 0 && 
        shipController.GetEnergy >= shipComponentController.requiredEnergy;

    public void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }
    public override void OnActivate()
    {        
        Debug.Log("Selecting Repair Target");
        MouseController.Instance.EnterTargetingMode(this);
    }

    public override void OnAgentActivate(TargetingData data)
    {
        OnTargetSelected(data);
    }

    public override void OnDeactivate()
    {
        
    }

    public override void OnTargetSelected(TargetingData target)
    {
        ShipComponentMeshController targetMesh = target.target;
        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        var targetShip = targetShipComponent.shipController;
        if (targetShipComponent.transform.parent != transform.parent)
        {
            Debug.Log("Wrong ship");
            shipComponentController.DeactivateComponent();
            return;
        }
        // Check that we can afford the repair and that the target is broken
        // In combat -> only broken components
        if (!targetShipComponent.broken)
        {
            Debug.Log("That component is not broken");
            shipComponentController.DeactivateComponent();
            return;
        }

        // do the repair
        Debug.Log("Clicking Repairs");
        targetShipComponent.RepairClick();       
        shipComponentController.DeactivateComponent();
        if (cooldown != null) cooldown.Trigger();
    }

    

    public override void ResetBehaviour()
    {
        
    }
}
