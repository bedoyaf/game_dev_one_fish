using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// spawns a shield on a ship component
/// Shield just spawns visualy, damage and collision is still handeled by shipcomponentcontroller
/// </summary>
public class ShieldComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] GameObject shieldPrefab;
    private int shieldsUp = 0;
    [SerializeField] private int maxAviableShields = 2;
    private List<Shield> shields = new List<Shield>();

    [SerializeField] GameObject physicalShieldPrefab;
    public bool usesPhysicalShields;
    private List<ShieldPhysical> physicalShields = new();
    private int physicalShieldsUp = 0;

    public SoundData shieldActivationClip;

    public override bool CanClickOnNow
    {
        get
        {
            
            return
                shipController != null &&
                cooldown.IsReady &&
            !shipComponentController.broken &&
            shipController.GetEnergy >= shipComponentController.requiredEnergy;
        }
    }
    public void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }
    public override bool OnActivate()
    {
        if(shieldsUp>=maxAviableShields)
        {
            Debug.Log("shield already placed");
            shipComponentController.DeactivateComponent();
            return false;
        }
        Debug.Log("Shield Up");
     //   SpawnShield();
        MouseController.Instance.EnterTargetingMode(this);
        //   shipComponentController.DeactivateComponent();
        return true;
    }

    public override void OnAgentActivate(TargetingData data)
    {
        OnTargetSelected(data);
    }

    public override bool OnDeactivate()
    {
        // No action 
        return false;
    }

    public override bool CancelTargeting()
    {
        MouseController.Instance.ExitTargetingMode(false);

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
        if (targetShipComponent.shield != null)
        {
            shipComponentController.DeactivateComponent();
            return false;
        }

        if (usesPhysicalShields)
            SpawnPhysicalShield(target);
        else
            SpawnShield(targetShipComponent);

        AudioManager.Instance.PlaySFX(shieldActivationClip);
        shipComponentController.DeactivateComponent();
        if (cooldown != null) cooldown.Trigger();

        return true;
    }

    private void SpawnShield(ShipComponentController target)
    {

        Transform targetTransform = target.transform;

        float offset = 0.5f;

        Vector3 position = new Vector3(targetTransform.position.x + offset, targetTransform.position.y + offset, targetTransform.position.z + offset);

        if (!shipComponentController.shipController.playerShip) position = new Vector3(targetTransform.position.x - offset, targetTransform.position.y + offset, targetTransform.position.z + offset);

        GameObject shieldObj = Instantiate(
        shieldPrefab, 
        position,
        targetTransform.rotation
        );
        shieldObj.transform.SetParent(targetTransform);

        Shield shield = shieldObj.GetComponent<Shield>();
        shields.Add( shield);
        shieldsUp ++;
        shield.OnShieldDestroyed.AddListener(OnShieldDestroyed);
        target.ActivateShield(shield);
    }

    private void SpawnPhysicalShield(TargetingData target) {

        float offset = 0.5f;
        var targetTransform = target.target.transform.parent;

        //Vector3 position = new Vector3(targetTransform.position.x + offset, targetTransform.position.y + offset, targetTransform.position.z + offset);
        Vector3 position = targetTransform.position + Vector3.one * offset + target.ComponentOffset;

        if (!shipComponentController.shipController.playerShip)
            position = targetTransform.position + new Vector3(-1, 0, 1) * offset + target.ComponentOffset;
            //position = new Vector3(targetTransform.position.x - offset, targetTransform.position.y + offset, targetTransform.position.z + offset);

        GameObject shieldObj = Instantiate(
        physicalShieldPrefab,
        position,
        targetTransform.rotation
        );
        //shieldObj.transform.SetParent(targetTransform);

        ShieldPhysical physicalShield = shieldObj.GetComponent<ShieldPhysical>();
        physicalShields.Add(physicalShield);
        physicalShieldsUp++;
        physicalShield.OnShieldDestroyed.AddListener(OnPhysicalShieldDestroyed);
        //target.ActivateShield(shield);
    }

    private void OnShieldDestroyed(Shield shield)
    {
        for(int i =0; i<shields.Count; i++)
        {
            if (shields[i] == shield)
            {
                Destroy(shields[i]);
                shields.Remove(shield);
                break;
            }
        }
        shieldsUp--;
    }

    private void OnPhysicalShieldDestroyed(ShieldPhysical physicalShield) {
        for (int i = 0; i < physicalShields.Count; i++) {
            if (physicalShields[i] == physicalShield) {
                Destroy(physicalShields[i]);
                physicalShields.Remove(physicalShield);
                break;
            }
        }
        physicalShieldsUp--;
    }

    public override void ResetBehaviour()
    {
        shieldsUp = 0;

        foreach(var shield in shields)
        {
            Destroy(shield);
        }
    }
}
