using UnityEngine;

public interface IShipComponentBehaviour
{
    void OnActivate();
    void OnTargetSelected(TargetingData data);
    void OnDeactivate();
}
