using UnityEngine;

[RequireComponent(typeof(ShipComponentController))]
public class ShieldComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] GameObject shieldPrefab;
    public override void OnActivate()
    {
        Debug.Log("Shield Up");
        SpawnShield();
        shipComponentController.DeactivateComponent();
    }

    public override void OnDeactivate()
    {
        Debug.Log("Shield off");
    }

    public override void OnTargetSelected(ShipComponentMeshController target)
    {

    }

    private void SpawnShield()
    {
        Transform ship = shipController.transform;

        GameObject shield = Instantiate(
        shieldPrefab,
        ship.position,
        ship.rotation
        );
        shield.transform.SetParent(ship);
    }
}
