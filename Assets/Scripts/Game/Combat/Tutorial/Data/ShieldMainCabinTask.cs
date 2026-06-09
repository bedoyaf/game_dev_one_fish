using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "ShieldMainCabinTask", menuName = "Scriptable Objects/Tasks/ShieldMainCabinTask")]
public class ShieldMainCabinTask : TutorialTaskSO
{
    [SerializeField] private float shotDelay = 3f;
    [SerializeField] private float pauseDelay = 2f;
    public override void BeginTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept();


        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Shield, ComponentType.Generator, ComponentType.MainCabin });

        List<MissileComponentController> component2 = GameManager.Instance.currentGameplayManager.EnemyShip.componentGrid.GetComponentsOfType<MissileComponentController>();
        component2[0].OnShotFired.AddListener(LaunchCoroutineForPause);

        List<ShieldComponentController> component = GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<ShieldComponentController>();


        component[0].OnShieldActivated.AddListener(EvaluateComponent);
        component[0].OnEnteredTargeting.AddListener(turnOfMostColliders);

        GameManager.Instance.StartCoroutine(DelayShot());
    }

    private void turnOfMostColliders()
    {
        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.MainCabin });
    }

    private void LaunchCoroutineForPause()
    {
        GameManager.Instance.StartCoroutine(PauseAfterDelay());
    }

    private IEnumerator PauseAfterDelay()
    {
        yield return MyTime.WaitForSeconds(pauseDelay);

        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0.3f);
        MyTime.pausedOverride = 0f;

        // MyTime.slowDownOverride = 0.1f;
    }

    private IEnumerator DelayShot()
    {
        yield return MyTime.WaitForSeconds(shotDelay);

        GameManager.Instance.currentGameplayManager.EnemyShip.GetComponent<EnemyShipAgent>().ShootAtComponent(
            GameManager.Instance.currentGameplayManager.playerShip.GetMainCabin()
        );
    }

    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }

    private void EvaluateComponent()
    {
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0f);
        MyTime.pausedOverride = 1f;
        CompleteTask();
    }
}