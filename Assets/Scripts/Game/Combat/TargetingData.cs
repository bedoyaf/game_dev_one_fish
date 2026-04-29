using UnityEngine;

public class TargetingData
{
    public ShipComponentMeshController target;
    public Vector3? direction;
    public Vector3? componentOffset; // Offset from the target transform position to correct component tile

    public TargetingData(ShipComponentMeshController target, Vector3? direction=null, Vector3? componentOffset = null)
    {
        this.target = target;
        this.direction = direction;
        this.componentOffset = componentOffset;
    }
}
