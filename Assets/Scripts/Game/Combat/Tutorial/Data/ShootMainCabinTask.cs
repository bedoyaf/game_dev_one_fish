using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "ShootMainCabinTask", menuName = "Scriptable Objects/Tasks/ShootMainCabinTask")]
public class ShootMainCabinTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        ShipComponentController component = GameManager.Instance.currentGameplayManager.EnemyShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.MainCabin });

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket, ComponentType.Generator});

        component.OnDeath.AddListener(EvaluateComponent);

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