using System;
using UnityEngine;

/// <summary>
/// Projectile class
/// </summary>
public class HookProbe : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 1f;

    
    private Action onHit;

    public void Init(Action onHit)
    {
        this.onHit = onHit;
        
        MyTime.ScheduleDestruction(gameObject, lifetime);        
    }

    void Update()
    {
        // not dependent on MyTime !!
        transform.position += speed * Time.deltaTime * transform.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Probe Hit: " + collision.gameObject.name);

        var target = collision.gameObject.GetComponent<ShipComponentMeshController>();

        if (target != null)
        {
            onHit();
        }

        Destroy(gameObject);
    }
}

