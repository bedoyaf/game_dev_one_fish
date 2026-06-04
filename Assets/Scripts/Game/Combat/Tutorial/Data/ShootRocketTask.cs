using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "ShootRocketTask", menuName = "Scriptable Objects/Tasks/ShootRocketTask")]
public class ShootRocketTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        List<MissileComponentController> component = GameManager.Instance.currentGameplayManager.EnemyShip.componentGrid.GetComponentsOfType<MissileComponentController>();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(ComponentType.Rocket);

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(ComponentType.Rocket, ComponentType.Generator);

        component[0].shipComponentController.OnDeath.AddListener(EvaluateComponent);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }

    private void EvaluateComponent(ShipComponentController component)
    {
        CompleteTask();
    }
}