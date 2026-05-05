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

    public ParticleSystem deathParticles;
    public float particlesLifetime = 3;

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;

        //Destroy(gameObject, lifetime);
        MyTime.ScheduleDestruction(gameObject, lifetime);
        var d = new Vector2(direction.x, direction.z).normalized;
        float angle = -(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        transform.localEulerAngles = new Vector3(0, angle, 0);
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

        var particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
        MyTime.ScheduleDestruction(particles.gameObject, particlesLifetime);

        Destroy(gameObject);
    }
}

