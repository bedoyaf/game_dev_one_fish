using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Projectile : MonoBehaviour
{
    public float waypointOffset = 10f;
    public float damage = 50;
    public float speed = 20f;
    public float turnSpeed = 5f;
    public float lifetime = 8f;

    private Vector3 currentDir;
    private List<Vector3> path = new List<Vector3>();
    private int currentIndex = 0;





    public void Init(Transform target)
    {
        path = BuildPath(target, FindBestAttackDirection(target.GetComponent<Collider>()));

        currentDir = (path[0] - transform.position).normalized;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (currentIndex >= path.Count)
            return;

        Vector3 targetPos = path[currentIndex];

        Vector3 desiredDir = (targetPos - transform.position).normalized;

        currentDir = Vector3.Lerp(currentDir, desiredDir, turnSpeed * Time.deltaTime).normalized;

        transform.position += currentDir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(currentDir);

        if (Vector3.Distance(transform.position, targetPos) < 1f)
        {
            currentIndex++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);

        var target = collision.gameObject.GetComponent<ShipComponentMeshController>();

        if (target != null)
        {
            target.OnDamagableCollision(damage);
        }

        Destroy(gameObject);
    }

    public static Vector3 FindBestAttackDirection(Collider targetCollider)
    {
        Vector3 center = targetCollider.bounds.center;

        Vector3[] directions = new Vector3[]
        {
        targetCollider.transform.forward,
        -targetCollider.transform.forward,
        targetCollider.transform.right,
        -targetCollider.transform.right
        };

        float bestDistance = float.MaxValue;
        Vector3 bestDir = directions[0];

        foreach (var dir in directions)
        {
            Ray ray = new Ray(center, dir);

            if (targetCollider.Raycast(ray, out RaycastHit hit, 100f))
            {
                float dist = hit.distance;

                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestDir = dir;
                }
            }
        }

        return bestDir;
    }

    private List<Vector3> BuildPath(Transform target, Vector3 desiredDir)
    {
        List<Vector3> result = new List<Vector3>();

        Vector3 center = target.position;
        float radius = 6f;

        Vector3 front = center + target.forward * radius;
        Vector3 back = center - target.forward * radius;
        Vector3 right = center + target.right * radius;
        Vector3 left = center - target.right * radius;

        Vector3 finalPoint = back;

        float distRight = Vector3.Distance(transform.position, right);
        float distLeft = Vector3.Distance(transform.position, left);

        if (distRight < distLeft)
        {
            result.Add(right);
        }
        else
        {
            result.Add(left);
        }

        result.Add(finalPoint);

        result.Add(center);

        return result;
    }
}

