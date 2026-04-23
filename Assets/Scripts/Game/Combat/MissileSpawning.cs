using Microsoft.Win32.SafeHandles;
using System.Net;
using UnityEngine;

public class MissileSpawning : MonoBehaviour
{
    [SerializeField]
    private Vector2 leftBottom;

    [SerializeField]
    private Vector2 widthHeight;

    public Vector3 GetWorldTop      => new(0, 0, transform.position.z + leftBottom.y + widthHeight.y);
    public Vector3 GetWorldBottom   => new(0, 0, transform.position.z + leftBottom.y);
    public Vector3 GetWorldLeft     => new(transform.position.x + leftBottom.x, 0, 0);
    public Vector3 GetWorldRight     => new(transform.position.x + leftBottom.x + widthHeight.x, 0, 0);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 size = new Vector3(widthHeight.x, 0, widthHeight.y);
        Vector3 pos = GetWorldLeft + GetWorldBottom + size * 0.5f;

        Gizmos.DrawWireCube(pos, size);
    }
}
