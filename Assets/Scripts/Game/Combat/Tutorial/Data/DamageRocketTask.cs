using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "DamageRocketTask", menuName = "Scriptable Objects/Tasks/DamageRocketTask")]
public class DamageRocketTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        List<MissileComponentController> component = GameManager.Instance.currentGameplayManager.EnemyShip.componentGrid.GetComponentsOfType<MissileComponentController>();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket });

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket, ComponentType.Generator });

        component[0].shipComponentController.OnDamage.AddListener(CompleteTask);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }
}