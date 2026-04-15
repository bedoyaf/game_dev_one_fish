using NUnit.Framework;
using UnityEngine;

/// <summary>
/// spawns a shield on a ship component
/// Shield just spawns visualy, damage and collision is still handeled by shipcomponentcontroller
/// </summary>
[RequireComponent(typeof(ShipComponentController))]
public class ShieldComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] GameObject shieldPrefab;
    private int shieldsUp = 0;
    [SerializeField] private int maxAviableShields = 2;
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

    public override void OnDeactivate()
    {
        
    }

    public override void OnTargetSelected(TargetingData target)
    {
        ShipComponentMeshController targetMesh = target.target;
        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        var targetShip = targetShipComponent.transform.parent.GetComponent<ShipController>();
        if (targetShipComponent.transform.parent != transform.parent)
        {
            Debug.Log("Wrong ship");
        }
        SpawnShield(targetShipComponent);
        shipComponentController.DeactivateComponent();
    }

    private void SpawnShield(ShipComponentController target)
    {
        if (target.shield != null) return;

        Transform targetTransform = target.transform;

        float offset = 0.5f;

        GameObject shieldObj = Instantiate(
        shieldPrefab,
        new Vector3(targetTransform.position.x+offset, targetTransform.position.y +offset, targetTransform.position.z+offset),
        targetTransform.rotation
        );
        shieldObj.transform.SetParent(targetTransform);

        Shield shield = shieldObj.GetComponent<Shield>();
        shieldsUp ++;
        shield.OnShieldDestroyed.AddListener(OnShieldDestroyed);
        target.ActivateShield(shield);
    }

    private void OnShieldDestroyed()
    {
        shieldsUp--;
    }
}
