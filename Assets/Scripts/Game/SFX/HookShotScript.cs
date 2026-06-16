using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HookShotScript : MonoBehaviour
{
    [SerializeField] private MainCabinComponentController ownCabin;

    // hook 
    [SerializeField] private GameObject hook;

    [SerializeField] private SpriteRenderer hookVisual;

    // hook's line
    [SerializeField] private LineRenderer hookLine;

    [SerializeField] private float grabScaleMult = 1.5f;

    [SerializeField] private HookProbe hookProbePrefab;

    [SerializeField] private Transform hookPivot;

    private Vector3 restPosition;
    private Vector3 restScale;

    [SerializeField] private ParticleSystem componentTearParticles;

    [Header("Sounds")]
    [SerializeField] private SoundData movingSound; 
    [SerializeField] private float movingSoundFadeDuration = 0.2f;
    [SerializeField] private SoundData tearingSound;

    private void Start()
    {
        restPosition = hook.transform.position;
        restScale = hook.transform.localScale;
    }

    [SerializeField] private HookIndicatorScript hookIndicator;

    public void HideHook()
    {
        hook.SetActive(false);
        hookLine.gameObject.SetActive(false);
        hookIndicator.gameObject.SetActive(false);
    }

    private bool following = true;
    private bool moving = false;
    private float moveSpeed = 0;
    private Vector3 nextDir = Vector3.zero;


    [SerializeField]
    private float angleRange = 20;
    [SerializeField]
    private float hookLen = 1;


    private float prevAngle = 0;
    private float hookSwing = 0;
    private float hookSwingVelocity = 0;

    void Update()
    {
        // always trying to move the hook
        if (moving)
        {
            hook.transform.position += moveSpeed * MyTime.deltaTime * nextDir;
        }
        else if(following && hook.activeSelf)
        {
            // Follow when aiming only // else just rotate slightly
            if(ownCabin.shipComponentController.activated)
            {
                // Scale up
                hook.transform.localScale = grabScaleMult * Vector3.one;

                hookVisual.transform.localRotation = Quaternion.Euler(0, 0, 0);

                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 screenPos = Camera.main.WorldToScreenPoint(hookPivot.transform.position);
                Vector2 offset = mousePosition - new Vector2(screenPos.x, screenPos.y);

                float angle = Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg;

                angle -= hook.transform.eulerAngles.y;

                hook.transform.RotateAround(hookPivot.position, hookPivot.forward, angle);
            }
            // rest position (slightly rotating)
            else
            {
                // rest scale
                hook.transform.localScale = restScale;

                // where want to be now 
                float angle = - 2*angleRange + Mathf.Cos(MyTime.time) * angleRange;

                float ropeAngularVelocity = Mathf.DeltaAngle(prevAngle, angle);
                hookSwing += -ropeAngularVelocity * 0.5f;

                hookSwingVelocity += -hookSwing * 20f * MyTime.deltaTime;
                hookSwingVelocity *= 0.98f;
                hookSwing += hookSwingVelocity * MyTime.deltaTime;

                float finalAngle = -90 + angle + hookSwing;

                prevAngle = angle;

                // rotate the rope attached point
                angle -= hook.transform.eulerAngles.y;
                hook.transform.RotateAround(hookPivot.position, hookPivot.forward, angle);

                // rotate the hook
                hookVisual.transform.localRotation = Quaternion.Euler(0, 0, finalAngle);
                
            }

        }

        // always trying to connect the line
        hookLine.SetPosition(0, hookPivot.position);
        hookLine.SetPosition(1, hook.transform.position);
    }


    // How long the hook takes to get to the destination
    [SerializeField] private float flyTime = 0.5f;

    // How long the game is slowed down, to see the effect of the grab
    [SerializeField] private float grabTime = 0.3f;

    // How long to wait for the hookProbe to hit
    [SerializeField] private float probeTime = 0.3f;

    [SerializeField] private ParticleSystem bubbleParticlesPrefab;
    //[SerializeField] private ParticleSystem tearParticlesPrefab;

    public void ShootHookAt(
        ShipComponentController component,
        Vector3 position,

        Predicate<bool> pullTowards,
        Action<bool> onFinished)
    {
        StartCoroutine(ShootHook(component, position.SetY(hook.transform.position.y), pullTowards, onFinished));
    }

    private IEnumerator ShootHook(
        ShipComponentController component,
        Vector3 position,

        Predicate<bool> pullTowards,
        Action<bool> onFinished)
    {
        // scale up
        hook.transform.localScale = grabScaleMult * Vector3.one;

        // Create bubbles
        var bubbles = Instantiate(bubbleParticlesPrefab, hook.transform);
        bubbles.transform.localPosition = Vector3.zero;
        //var bubblesMain = bubbles.main;

        // fly there
        // shorten by some length (of the hook)
        nextDir = (position - hook.transform.position);
        moveSpeed = (nextDir.magnitude - hookLen) / flyTime;
        nextDir = nextDir.normalized;

        following = false;
        moving = true;

        var movementAudio = AudioManager.Instance.PlaySFX(movingSound, hook.transform.position, 10);
        movementAudio.volume = 0;
        movementAudio.loop = true;
        movementAudio.DOFade(1, movingSoundFadeDuration);

        // prefire the probe slightly before there
        yield return MyTime.WaitForSeconds(flyTime - probeTime);

        // Fire a hook probe and wait for it to hit
        bool hookProbeHit = false;

        HookProbe hookProbe = Instantiate(
            hookProbePrefab,
            position + 2f * Vector3.up,
            Quaternion.LookRotation(Vector3.down)
        );

        hookProbe.Init(() => { hookProbeHit = true; });

        yield return MyTime.WaitForSeconds(probeTime);

        // There and probe is done
        var goalPos = position - nextDir * hookLen;
        hook.transform.position = goalPos;
        moving = false;

        // Maybe rotate the hook / move back a pixel or two, as if pulling
        hook.transform.DOMove(goalPos + 0.2f * (restPosition - goalPos).normalized, grabTime * 0.8f);

        // SFX
        movementAudio.DOFade(0, movingSoundFadeDuration);
        AudioManager.Instance.PlaySFX(tearingSound);
        bubbles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        //var tearParticles = Instantiate(tearParticlesPrefab, component.GetComponentCenter().SetY(2f), Quaternion.identity);

        yield return MyTime.WaitForSeconds(grabTime);
        var componentOriginalPosition = component.GetComponentCenter();

        // reparent the component temporarily to the hook
        var pull = pullTowards(hookProbeHit);
        if (pull) {
            component.gameObject.transform.SetParent(hook.transform);
            Instantiate(componentTearParticles, componentOriginalPosition, Quaternion.identity);
            component.RemoveShield();
            component.shipController.CheckFailState();
        }

        // fly back

        nextDir = (restPosition - hook.transform.position);
        moveSpeed = nextDir.magnitude / flyTime;
        nextDir = nextDir.normalized;

        moving = true;

        movementAudio.DOFade(1, movingSoundFadeDuration);

        bubbles.transform.localEulerAngles = bubbles.transform.localEulerAngles.SetX(180); // Flip bubbles
        bubbles.Play(false);
        //if (tearParticles.TryGetComponent<DetachableParticles>(out var detachable)) {
        //    detachable.Detach();
        //}

        yield return MyTime.WaitForSeconds(flyTime);
        
        hook.transform.position = restPosition;
        moving = false;
        following = true;

        onFinished(pull);

        // reset scale
        hook.transform.localScale = restScale;

        movementAudio.DOFade(0, movingSoundFadeDuration);
        if (bubbles.TryGetComponent<DetachableParticles>(out var detachable)) {
            detachable.Detach();
        }
    }

}
