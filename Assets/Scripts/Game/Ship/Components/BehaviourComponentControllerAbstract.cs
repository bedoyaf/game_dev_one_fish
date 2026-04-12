using UnityEngine;

public abstract class BehaviourComponentControllerAbstract : MonoBehaviour, IShipComponentBehaviour
{
    protected ShipController shipController;
    protected ShipComponentController shipComponentController;

    public void Awake()
    {
        shipController = GetComponentInParent<ShipController>();
        shipComponentController = GetComponent<ShipComponentController>();
    }

    public abstract void OnActivate();
    public abstract void OnDeactivate();
    public abstract void OnTargetSelected(ShipComponentMeshController target);
}
