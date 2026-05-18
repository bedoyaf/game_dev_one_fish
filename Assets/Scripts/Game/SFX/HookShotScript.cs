using DG.Tweening;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using UnityEngine;

public class HookShotScript : MonoBehaviour
{
    // hook 
    [SerializeField] private GameObject hook;

    // hook's line
    [SerializeField] private LineRenderer hookLine;

    [SerializeField] private float grabScaleMult = 1.5f;

    private Vector3 restPosition;
    private Vector3 restScale;
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

    private bool moving = false;
    private float moveSpeed = 0;
    private Vector3 nextDir = Vector3.zero;


    void Update()
    {
        // always trying to move the hook
        if(moving)
        {
            hook.transform.position += moveSpeed * MyTime.deltaTime * nextDir;
        }

        // always trying to connect the line
        hookLine.SetPosition(0, hookLine.transform.position);
        hookLine.SetPosition(1, hook.transform.position);
    }


    // How long the hook takes to get to the destination
    [SerializeField] private float flyTime = 0.5f;

    // How long the game is slowed down, to see the effect of the grab
    [SerializeField] private float grabTime = 0.3f;
    public void ShootHookAt(
        ShipComponentController component, 
        Vector3 position, 
        
        bool pullTowards,
        Action onGrab, 
        Action onFinished)
    {
        StartCoroutine(ShootHook(component, position.SetY(hook.transform.position.y), pullTowards, onGrab, onFinished));
    }

    private IEnumerator ShootHook(
        ShipComponentController component, 
        Vector3 position, 
        
        bool pullTowards,
        Action onGrab, 
        Action onFinished)
    {
        // scale up
        hook.transform.localScale = grabScaleMult * Vector3.one;

        // fly there

        nextDir = (position - hook.transform.position);
        moveSpeed = nextDir.magnitude / flyTime;
        nextDir = nextDir.normalized;

        moving = true;

        yield return MyTime.WaitForSeconds(flyTime);

        hook.transform.position = position;
        moving = false;

        // Maybe rotate the hook / move back a pixel or two, as if pulling

        hook.transform.DOMove(position + 0.2f * (restPosition - position).normalized, grabTime * 0.8f);

        yield return MyTime.WaitForSeconds(grabTime);

        onGrab();

        // reparent the component temporarily to the hook
        if(pullTowards)
            component.gameObject.transform.SetParent(hook.transform);

        // fly back

        nextDir = (restPosition - hook.transform.position);
        moveSpeed = nextDir.magnitude / flyTime;
        nextDir = nextDir.normalized;

        moving = true;

        yield return MyTime.WaitForSeconds(flyTime);

        hook.transform.position = restPosition;
        moving = false;

        onFinished();

        // reset scale
        hook.transform.localScale = restScale;
    }

}
