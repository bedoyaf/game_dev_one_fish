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

    public void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }
    public override void OnActivate()
    {
        if(shieldsUp>=maxAviableShields)
        {
            Debug.Log("shield already placed");
            shipComponentController.DeactivateComponent();
            return;
        }
        Debug.Log("Shield Up");
     //   SpawnShield();
        MouseController.Instance.EnterTargetingMode(this);
     //   shipComponentController.DeactivateComponent();
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
        if (targetShipComponent.shield != null)
        {
            shipComponentController.DeactivateComponent();
            return;
        }

        SpawnShield(targetShipComponent);
        shipComponentController.DeactivateComponent();
        if (cooldown != null) cooldown.Trigger();
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

    public override void ResetBehaviour()
    {
        shieldsUp = 0;

        foreach(var shield in shields)
        {
            Destroy(shield);
        }
    }
}
