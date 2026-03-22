using UnityEngine;
using UnityEngine.Events;


public class ShipComponentController : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent OnDeath;
    public bool activated = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ActivateComponents()
    {

    }

    public void DeactivateComponents()
    {

    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
}
