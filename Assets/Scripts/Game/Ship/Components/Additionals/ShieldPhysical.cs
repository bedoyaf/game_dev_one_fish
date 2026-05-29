using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shield meant to physically exist in the space.
/// </summary>
public class ShieldPhysical : MonoBehaviour
{
    [SerializeField] private float health = 10;
    [SerializeField] private float lifeSpan = 5;
    [SerializeField] private float shieldFadeTime = 0.2f;
    [SerializeField] private SoundData shieldEndClip;

    public UnityEvent<ShieldPhysical> OnShieldDestroyed;

    private Material shieldMaterial;
    private bool dieCalled = false;

    private void Awake() {
        shieldMaterial = GetComponent<MeshRenderer>().material;
        shieldMaterial.SetFloat("_Progress", 0.0f);
        shieldMaterial.DOFloat(1, "_Progress", shieldFadeTime);
    }

    public void Start()
    {
        //MyTime.ScheduleDestruction(gameObject, lifeSpan);
        StartCoroutine(ScheduleDeath(lifeSpan));
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
        // Disable all colliders
        var colliders = GetComponentsInChildren<Collider>();
        foreach(Collider collider in colliders) {
            collider.enabled = false;
        }

        // Fade the shield
        StartCoroutine(DieCoroutine());
        //Destroy(gameObject);
    }

    private IEnumerator ScheduleDeath(float lifeSpan) {
        yield return MyTime.WaitForSeconds(lifeSpan);
        if (this != null && gameObject != null) {
            yield return DieCoroutine();
        }
    }

    private IEnumerator DieCoroutine() {
        if (dieCalled) yield break;
        dieCalled = true;

        AudioManager.Instance.PlaySFX(shieldEndClip, transform.position);
        shieldMaterial.DOFloat(0, "_Progress", shieldFadeTime);
        yield return new WaitForSeconds(shieldFadeTime);
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
