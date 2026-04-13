using UnityEngine;


[RequireComponent(typeof(ShipComponentController))]
public class MissileComponentController : BehaviourComponentControllerAbstract 
{
    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform missileSpawnPoint;

    public override void OnActivate()
    {
        Debug.Log("Missile ready");

        MouseController.Instance.EnterTargetingMode(this);
    }

    public override void OnDeactivate()
    {
        Debug.Log("Missile offline");
    }

    public override void OnTargetSelected(ShipComponentMeshController target)
    {
        ShipComponentController targetShipComponent = target.transform.parent.GetComponent<ShipComponentController>();
        var targetShip = targetShipComponent.transform.parent.GetComponent<ShipController>();
        if(targetShipComponent.transform.parent == transform.parent )
        {
            Debug.Log("Wrong ship");
            OnDeactivate();
        }
        Debug.Log("Target selected: " + target.name);

        Shoot(target, targetShip);

        shipComponentController.DeactivateComponent();
      //  MouseController.Instance.ClearClickAction();
    }

    public void Shoot(ShipComponentMeshController target, ShipController targetShip)
    {
        Debug.Log("Missile launched at " + target.name);

        Vector3 spawnPos = missileSpawnPoint.position;

        Vector3 targetPos = target.GetComponent<Collider>().bounds.center;

        Vector3 dir = (targetPos - spawnPos).normalized;

        GameObject missile = Instantiate(missilePrefab, spawnPos, Quaternion.LookRotation(dir));

        Projectile proj = missile.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Init( target.transform);
        }
    }
}

