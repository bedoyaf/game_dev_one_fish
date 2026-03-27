using UnityEngine;

public interface IShipComponentBehaviour
{
    void OnActivate();
    void OnTargetSelected(ShipComponentMeshController target);
    void OnDeactivate();
}
