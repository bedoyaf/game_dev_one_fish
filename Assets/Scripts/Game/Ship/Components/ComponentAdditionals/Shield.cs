using UnityEngine;

public class Shield : MonoBehaviour, IDamagableCollider
{
    [SerializeField] private float health = 100;
    [SerializeField] private float lifeSpan = 5;

    public void Start()
    {
        Destroy(gameObject, lifeSpan);
    }
    public void OnDamagableCollision(float amount)
    {
        TakeDamage(amount);
    }

    private void TakeDamage(float dmg)
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
}
