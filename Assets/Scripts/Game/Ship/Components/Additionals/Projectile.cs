using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile class
/// </summary>
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;
    public int damage = 5;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;

        //Destroy(gameObject, lifetime);
        MyTime.ScheduleDestruction(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * MyTime.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
      //  Debug.Log("Hit: " + collision.gameObject.name);

        var target = collision.gameObject.GetComponent<ShipComponentMeshController>();

        if (target != null)
        {
            target.OnDamagableCollision(damage);
        }

        Destroy(gameObject);
    }
}

