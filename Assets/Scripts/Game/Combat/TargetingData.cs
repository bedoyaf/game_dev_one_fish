using UnityEngine;

public class TargetingData
{
    public ShipComponentMeshController target;
    public Vector3? direction;

    // Offset from the target transform position to correct component tile
    private Vector3? componentOffset; 
    public Vector3 ComponentOffset => componentOffset == null ? Vector3.zero : componentOffset.Value;

    public Vector3 ExactTargetPosition => target.transform.parent.position + target.transform.parent.transform.right * 0.5f + ComponentOffset;

    //public Vector3 ShieldPosition(float offset) {

    //}
    
    public TargetingData(ShipComponentMeshController target, Vector3? direction=null, Vector3? componentOffset = null)
    {
        this.target = target;
        this.direction = direction;
        this.componentOffset = componentOffset;
    }
}
