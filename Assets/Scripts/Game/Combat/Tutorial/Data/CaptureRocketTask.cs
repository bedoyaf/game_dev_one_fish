using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "CaptureRocketTask", menuName = "Scriptable Objects/Tasks/CaptureRocketTask")]
public class CaptureRocketTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        ShipComponentController component = GameManager.Instance.currentGameplayManager.playerShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(ComponentType.Rocket);

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(ComponentType.MainCabin, ComponentType.Generator);

        ((MainCabinComponentController)component.componentBehaviour).onComponentPickup.AddListener(CompleteTask);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }

    /*private void EvaluateComponent(ShipComponentController component)
    {
        CompleteTask();
    }*/
}