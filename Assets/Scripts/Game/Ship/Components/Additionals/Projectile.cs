using System.Collections;
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

    public SoundData deathSound;
    public ParticleSystem deathParticles;
    public float particlesLifetime = 3;

    private Vector3 direction;
    private bool incomingNotified = false;
    private float maxRange = 0f;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;

        //Destroy(gameObject, lifetime);
        MyTime.ScheduleDestruction(gameObject, lifetime);
        maxRange = speed * lifetime;
        var d = new Vector2(direction.x, direction.z).normalized;
        float angle = -(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        transform.localEulerAngles = new Vector3(0, angle, 0);
    }

    void Update()
    {
        // Before moving, raycast along the remaining path once to detect if we're going to hit a component.
        if (!incomingNotified)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, maxRange))
            {
                if (hit.collider != null && hit.collider.TryGetComponent<ShipComponentMeshController>(out var meshController))
                {
                    float distance = hit.distance;
                    float timeToImpact = distance / Mathf.Max(0.0001f, speed);
                    meshController.OnIncomingProjectile(hit.point, timeToImpact, this);
                    incomingNotified = true;
                }
            }
        }

        transform.position += direction * speed * MyTime.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //  Debug.Log("Hit: " + collision.gameObject.name);
        bool shouldDestroy = true;
        
        if (collision.gameObject.TryGetComponent<ShipComponentMeshController>(out var target))
        {
            StartCoroutine(DamageComponentCoroutine(target));
            shouldDestroy = false;
        }

        var particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
        MyTime.ScheduleDestruction(particles.gameObject, particlesLifetime);
        AudioManager.Instance.PlaySFX(deathSound, transform.position);

        if (shouldDestroy)
            Destroy(gameObject);
    }

    /// <summary>
    /// Do not question
    /// Just give some time for physics to realize it hit a shield
    /// </summary>
    private IEnumerator DamageComponentCoroutine(ShipComponentMeshController target) {
        yield return null;

        target.OnDamagableCollision(damage);
        if (gameObject != null) {
            Destroy(gameObject);
        }
    }
}

