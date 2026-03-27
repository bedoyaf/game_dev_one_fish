using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Projectile : MonoBehaviour
{
    public float damage = 50;
    public float speed = 20f;
    public float lifetime = 5f;
    private Vector3 direction;

    public void Init(Vector3 dir, ShipComponentMeshController target)
    {
        direction = dir.normalized;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);

        var target = collision.gameObject.GetComponentInParent<ShipComponentMeshController>();

        if (target != null)
        {
            target.OnDamagableCollision(damage);
        }

        Destroy(gameObject);
    }
}
