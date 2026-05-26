using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HookShotScript : MonoBehaviour
{
    [SerializeField] private MainCabinComponentController ownCabin;

    // hook 
    [SerializeField] private GameObject hook;

    // hook's line
    [SerializeField] private LineRenderer hookLine;

    [SerializeField] private float grabScaleMult = 1.5f;

    [SerializeField] private HookProbe hookProbePrefab;

    [SerializeField] private Transform hookPivot;

    private Vector3 restPosition;
    private Vector3 restScale;

    [Header("Sounds")]
    [SerializeField] private AudioClip movingSound;
    [SerializeField] private float movingSoundFadeDuration = 0.2f;
    [SerializeField] private AudioClip tearingSound;

    private void Start()
    {
        restPosition = hook.transform.position;
        restScale = hook.transform.localScale;
    }

    public void HideHook()
    {
        hook.SetActive(false);
        hookLine.gameObject.SetActive(false);
    }

    private bool following = true;
    private bool moving = false;
    private float moveSpeed = 0;
    private Vector3 nextDir = Vector3.zero;



    void Update()
    {
        // always trying to move the hook
        if (moving)
        {
            hook.transform.position += moveSpeed * MyTime.deltaTime * nextDir;
        }
        else if(following)
        {
            // indicate that clickable, by moving around to match
            // the screen mouse position
            if(hook.activeSelf && ownCabin.CanClickOnNow)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 screenPos = Camera.main.WorldToScreenPoint(hookPivot.transform.position);
                Vector2 offset = mousePosition - new Vector2(screenPos.x, screenPos.y);

                float angle = Mathf.Atan2(-offset.y, offset.x) * Mathf.Rad2Deg;

                angle -= hook.transform.eulerAngles.y;

                hook.transform.RotateAround(hookPivot.position, hookPivot.forward, angle);
            } 
            // rest position
            else
            {
                hook.transform.position = restPosition;
            }

        }

        // always trying to connect the line
        hookLine.SetPosition(0, hookLine.transform.position);
        hookLine.SetPosition(1, hook.transform.position);
    }


    // How long the hook takes to get to the destination
    [SerializeField] private float flyTime = 0.5f;

    // How long the game is slowed down, to see the effect of the grab
    [SerializeField] private float grabTime = 0.3f;

    // How long to wait for the hookProbe to hit
    [SerializeField] private float probeTime = 0.3f;

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

        // fly there

        nextDir = (position - hook.transform.position);
        moveSpeed = nextDir.magnitude / flyTime;
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
        hook.transform.position = position;
        moving = false;

        // Maybe rotate the hook / move back a pixel or two, as if pulling
        hook.transform.DOMove(position + 0.2f * (restPosition - position).normalized, grabTime * 0.8f);

        movementAudio.DOFade(0, movingSoundFadeDuration);
        AudioManager.Instance.PlaySFX(tearingSound);

        yield return MyTime.WaitForSeconds(grabTime);

        // reparent the component temporarily to the hook
        var pull = pullTowards(hookProbeHit);
        if (pull)
            component.gameObject.transform.SetParent(hook.transform);

        // fly back

        nextDir = (restPosition - hook.transform.position);
        moveSpeed = nextDir.magnitude / flyTime;
        nextDir = nextDir.normalized;

        moving = true;

        movementAudio.DOFade(1, movingSoundFadeDuration);

        yield return MyTime.WaitForSeconds(flyTime);
        
        hook.transform.position = restPosition;
        moving = false;
        following = true;

        onFinished(pull);

        // reset scale
        hook.transform.localScale = restScale;

        movementAudio.DOFade(0, movingSoundFadeDuration);
    }

}
