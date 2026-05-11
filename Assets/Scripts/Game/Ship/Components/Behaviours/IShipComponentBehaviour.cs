using UnityEngine;

public interface IShipComponentBehaviour
{
    bool OnActivate();
    bool OnTargetSelected(TargetingData data);
    public abstract void OnAgentActivate(TargetingData target);
    public void ResetBehaviour();
    bool OnDeactivate();
}
