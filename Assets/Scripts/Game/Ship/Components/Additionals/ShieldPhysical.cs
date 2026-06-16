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
    [SerializeField] private SoundData shieldEndClip;

    [Header("Shader settings")]
    [SerializeField] private float shieldFadeTime = 0.2f;
    [SerializeField] private float shieldOverFadeTime = 0.2f;
    [SerializeField] private float shieldOverSize = 1.5f;

    [SerializeField] private float hurtFadeTime = 0.2f;
    [SerializeField] private float hurtMaxDistance = 1f;
    [SerializeField] private SoundData hitSound;

    public UnityEvent<ShieldPhysical> OnShieldDestroyed;

    private Material shieldMaterial;
    private bool dieCalled = false;

    private float creationTime;

    private void Awake() {
        shieldMaterial = GetComponent<MeshRenderer>().material;
        shieldMaterial.SetFloat("_Progress", 0.0f);
        shieldMaterial.DOFloat(1f, "_Progress", shieldFadeTime);
        shieldMaterial.SetFloat("_VisualScale", 0.8f);
        shieldMaterial.SetFloat("_TimeOffset", Random.Range(0.0f, 100f));
        Sequence sequence = DOTween.Sequence();
        sequence.Append(shieldMaterial.DOFloat(shieldOverSize, "_VisualScale", shieldFadeTime)/*.SetEase(Ease.OutCubic)*/);
        sequence.Append(shieldMaterial.DOFloat(1, "_VisualScale", shieldOverFadeTime).SetEase(Ease.Linear)/*.SetLoops(2, LoopType.Yoyo)*//*.SetEase(Ease.InCubic)*/);
        creationTime = MyTime.time;
    }

    public void Start()
    {
        //MyTime.ScheduleDestruction(gameObject, lifeSpan);
        StartCoroutine(ScheduleDeath(lifeSpan));
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        AudioManager.Instance.PlaySFX(hitSound);
        if (health <= 0)
        {
            Die();
        }

        if (MyTime.time - creationTime < 0.2f) {
            GameManager.Instance.SFXManager.SetFishFace(Moods.ThisIsFine);
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
       // Debug.Log("Shield hit: " + collision.gameObject.name);

        var projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null) {
            TakeDamage(projectile.damage);
            if (health > 0) {
                shieldMaterial.SetVector("_HurtPosition", collision.GetContact(0).point);
                shieldMaterial.DOFloat(hurtMaxDistance, "_HurtDistance", hurtFadeTime).SetLoops(2, LoopType.Yoyo);
            }
            projectile.damage -= (int)health;
            projectile.damage = Mathf.Max(projectile.damage, 0);
        }
    }
}
