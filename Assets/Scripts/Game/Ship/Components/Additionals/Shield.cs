using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script for the projectile prefab, protects one component, has no collision mainly visual and to keep track of health
/// </summary>
public class Shield : MonoBehaviour
{
    [SerializeField] private float health = 10;
    [SerializeField] private float lifeSpan = 5;

    public UnityEvent<Shield> OnShieldDestroyed;

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
}
