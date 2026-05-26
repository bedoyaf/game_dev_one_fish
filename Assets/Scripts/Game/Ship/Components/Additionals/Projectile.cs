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

    public AudioClip deathSound;
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

    private bool dead = false;

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

