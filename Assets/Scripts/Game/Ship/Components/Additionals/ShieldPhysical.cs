using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shield meant to physically exist in the space.
/// </summary>
public class ShieldPhysical : MonoBehaviour
{
    [SerializeField] private float health = 10;
    [SerializeField] private float lifeSpan = 5;

    public UnityEvent<ShieldPhysical> OnShieldDestroyed;

    public void Start()
    {
        //Destroy(gameObject, lifeSpan);
        MyTime.ScheduleDestruction(gameObject, lifeSpan);
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnShieldDestroyed?.Invoke(this);
    }
    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Shield hit: " + collision.gameObject.name);

        var projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null) {
            TakeDamage(projectile.damage);
            projectile.damage -= (int)health;
            projectile.damage = Mathf.Max(projectile.damage, 0);
        }
    }
}
