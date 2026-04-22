using Unity.VisualScripting;
using UnityEngine;



public class MissileComponentController : BehaviourComponentControllerAbstract 
{
    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform missileSpawnPoint;

    private void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }

    public override void OnActivate()
    {
        Debug.Log("Missile ready");

        MouseController.Instance.EnterTargetingMode(this);
    }

    public override void OnAgentActivate(TargetingData data)
    {
        OnTargetSelected(data);
    }

    public override void OnDeactivate()
    {
        Debug.Log("Missile offline");
    }

    public override void OnTargetSelected(TargetingData target)
    {
        var targetMesh = target.target;
        Vector3 dir = new Vector3(target.direction.Value.x, target.direction.Value.y, target.direction.Value.z);

        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        ShipController targetShip = targetShipComponent.shipController;

        if (targetShipComponent.transform.parent == transform.parent)
        {
            Debug.Log("Wrong ship");
            shipComponentController.DeactivateComponent();
            return;
        }
        Vector3 exactTargetPosition = targetShipComponent.transform.position;
        Shoot(targetShip, dir, exactTargetPosition);

        shipComponentController.DeactivateComponent();
    }

    public void Shoot(ShipController targetShip, Vector3 dir, Vector3 targetPos)
    {
        dir = dir.normalized;
        float dotUp = Vector3.Dot(dir, Vector3.up);
        float dotRight = Vector3.Dot(dir, Vector3.right);

        Transform spawnPoint;
        Vector3 spawnPos;
        Vector3 shootDir;

        //offset due to targeting issues the colider not being perfectly on the spot
        float offset = 0.5f;

        if (Mathf.Abs(dotUp) > Mathf.Abs(dotRight))
        {
            if (dotUp > 0)
            {
                spawnPoint = targetShip.UpProjectileSpawn;
                shootDir = new Vector3(0,0,-1);
            }
            else
            {
                spawnPoint = targetShip.DownProjectileSpawn;
                shootDir = new Vector3(0, 0, 1);
            }

            spawnPos = new Vector3(targetPos.x-offset, 0+offset, spawnPoint.position.z);
        }
        else
        {
            if (dotRight < 0)
            {
                spawnPoint = targetShip.RightProjectileSpawn;
                shootDir = new Vector3(-1, 0, 0);
            }
            else
            {
                spawnPoint = targetShip.LeftProjectileSpawn;
                shootDir = new Vector3(1, 0, 0);
            }
            spawnPos = new Vector3(spawnPoint.position.x, 0+offset, targetPos.z+offset);
        }


        GameObject missile = Instantiate(
            missilePrefab,
            spawnPos,
            Quaternion.LookRotation(shootDir)
        );

        Projectile proj = missile.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Init(shootDir);
        }


        if(cooldown!=null) cooldown.Trigger();
    }
}

