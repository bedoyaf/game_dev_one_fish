using System.Collections;
using UnityEngine;



public class MissileComponentController : BehaviourComponentControllerAbstract 
{
    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform missileSpawnPoint;

    // How long, before the actual missile is spawned (the visual takes this long)
    [SerializeField] private float missileTravelTime;

    [SerializeField] private SoundData missileShootClip;

    public override bool CanClickOnNow
    {
        get
        {
            // if (cooldown == null) Debug.LogError("cooldown is NULL");
            // if (shipComponentController == null) Debug.LogError("shipComponentController is NULL");
            // if (shipController == null) Debug.LogError("shipController is NULL ");

            return cooldown != null &&
                   shipComponentController != null &&
                   shipController != null &&
                   cooldown.IsReady &&
                   !shipComponentController.broken &&
                   shipController.GetEnergy >= shipComponentController.requiredEnergy;
        }
    }

    private void Start()
    {
        cooldown = GetComponent<ComponentCooldown>();
    }

    public override bool OnActivate()
    {
    //    Debug.Log("Missile ready");
        MouseController.Instance.EnterTargetingMode(this);
        
        return true;
    }

    public override void OnAgentActivate(TargetingData data)
    {
        OnTargetSelected(data);

        var cooldown = GetComponent<ComponentCooldown>();
        if (cooldown != null) cooldown.Trigger();

        shipComponentController.DeactivateComponent();
    }

    public override bool OnDeactivate()
    {
        //       Debug.Log("Missile offline");
        return true;
    }

    public override bool CancelTargeting()
    {
        // If this behaviour entered targeting mode, ensure the mouse controller exits it
        // and refund the activation energy to the ship.
        MouseController.Instance.ExitTargetingMode(false);

        return true;
    }

    public override bool OnTargetSelected(TargetingData target)
    {
        var targetMesh = target.target;
        Vector3 dir = new Vector3(target.direction.Value.x, target.direction.Value.y, target.direction.Value.z);

        ShipComponentController targetShipComponent = targetMesh.transform.parent.GetComponent<ShipComponentController>();
        ShipController targetShip = targetShipComponent.shipController;

        if (targetShipComponent.transform.parent == transform.parent)
        {
            Debug.Log("Wrong ship");
            shipComponentController.DeactivateAimingAndRefund();
            return false;
        }

        Vector3 exactTargetPosition = targetShipComponent.transform.position + targetShipComponent.transform.right * 0.5f + target.ComponentOffset;
        // + targetShipComponent.gameObject.transform.parent.gameObject.transform.parent.gameObject.transform.localScale.x * new Vector3(0.5f, 0.0f, 0.5f);
        exactTargetPosition = target.ExactTargetPosition;
        Shoot(targetShip, dir, exactTargetPosition);

        shipComponentController.DeactivateComponent();

        return true;
    }

    public void Shoot(ShipController targetShip, Vector3 dir, Vector3 targetPos)
    {
        dir = dir.normalized;
        float dotUp = Vector3.Dot(dir, Vector3.up);
        float dotRight = Vector3.Dot(dir, Vector3.right);

        Vector3 spawnPoint;
        Vector3 spawnPos;
        Vector3 shootDir;

        if (Mathf.Abs(dotUp) > Mathf.Abs(dotRight))
        {
            if (dotUp > 0)
            {
                spawnPoint = targetShip.missileSpawnPoints.GetWorldTop;
                shootDir = new Vector3(0,0,-1);
            }
            else
            {
                spawnPoint = targetShip.missileSpawnPoints.GetWorldBottom;
                shootDir = new Vector3(0, 0, 1);
            }

            spawnPos = new Vector3(targetPos.x, 0.5f, spawnPoint.z);
        }
        else
        {
            if (dotRight < 0)
            {
                spawnPoint = targetShip.missileSpawnPoints.GetWorldRight;
                shootDir = new Vector3(-1, 0, 0);
            }
            else
            {
                spawnPoint = targetShip.missileSpawnPoints.GetWorldLeft;
                shootDir = new Vector3(1, 0, 0);
            }
            spawnPos = new Vector3(spawnPoint.x, 0.5f, targetPos.z+0.5f);
        }

        if (cooldown != null) cooldown.Trigger();

        // Spawn the visual immediately, the rocket only after a delay
        // based on travel speed
        AudioManager.Instance.PlaySFX(missileShootClip, missileSpawnPoint.position);
        GameManager.Instance.SFXManager.SpawnRocket(missileSpawnPoint.position, spawnPos, missileTravelTime);
        StartCoroutine(SpawnRocket(spawnPos, shootDir));

        /*
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
        */

    }

    IEnumerator SpawnRocket(Vector3 spawnPos, Vector3 shootDir)
    {
        //yield return new WaitForSeconds(missileTravelTime);
        yield return MyTime.WaitForSeconds(missileTravelTime);

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
    }
}

