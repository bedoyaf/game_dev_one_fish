using UnityEngine;

/// <summary>
/// spawns a shield on a ship component
/// </summary>
[RequireComponent(typeof(ShipComponentController))]
public class ShieldComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] GameObject shieldPrefab;
    private bool shieldUp = false;
    public override void OnActivate()
    {
        if(shieldUp)
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
        Debug.Log("Shield off");
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
        Transform targetTransform = target.transform;

        float offset = 0.5f;

        GameObject shieldObj = Instantiate(
        shieldPrefab,
        new Vector3(targetTransform.position.x+offset, targetTransform.position.y +offset, targetTransform.position.z+offset),
        targetTransform.rotation
        );
        shieldObj.transform.SetParent(targetTransform);

        Shield shield = shieldObj.GetComponent<Shield>();
        shieldUp = true;
        shield.OnShieldDestroyed.AddListener(OnShieldDestroyed);
        shipComponentController.ActivateShield(shield);
    }

    private void OnShieldDestroyed()
    {
        shieldUp = false;
    }
}
