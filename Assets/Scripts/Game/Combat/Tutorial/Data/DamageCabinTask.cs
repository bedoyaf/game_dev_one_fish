using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "DamageCabinTask", menuName = "Scriptable Objects/Tasks/DamageCabinTask")]
public class DamageCabinTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        var component = GameManager.Instance.currentGameplayManager.EnemyShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(ComponentType.MainCabin);

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(ComponentType.Rocket, ComponentType.Generator);

        component.OnDamage.AddListener(CompleteTask);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }

}