using UnityEngine;

public class TargetingData
{
    public ShipComponentMeshController target;
    public Vector3 direction;

    public TargetingData( ShipComponentMeshController target , Vector3 direction)
    {
        this.target = target;
        this.direction = direction;
    }
}
