using UnityEngine;

public interface IShipComponentBehaviour
{
    void OnActivate();
    void OnTargetSelected(TargetingData data);
    public abstract void OnAgentActivate(TargetingData target);
    public void ResetBehaviour();
    void OnDeactivate();
}
