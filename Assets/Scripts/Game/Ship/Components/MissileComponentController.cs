using UnityEngine;

public class MissileComponentController : MonoBehaviour, IShipComponentBehaviour
{
    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform missileSpawnPoint;
    private ShipController shipController;  
    private ShipComponentController shipComponentController;

    public void OnActivate()
    {
        Debug.Log("Missile ready");

        // zapni targeting mode
        MouseController.Instance.EnterTargetingMode(this);
        if (shipController == null) shipController = transform.parent.GetComponent<ShipController>();
    }

    public void OnDeactivate()
    {
        Debug.Log("Missile offline");

       // MouseController.Instance.ClearClickAction();
        if(shipComponentController == null ) shipComponentController = GetComponent<ShipComponentController>();
        if (shipController == null) shipController = transform.parent.GetComponent<ShipController>();
    }

    public void OnTargetSelected(ShipComponentMeshController target)
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

        // po výstøelu vypni targeting
        if(shipComponentController ==null) shipComponentController = GetComponent<ShipComponentController>();
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
            proj.Init(dir, target);
        }
    }
}

