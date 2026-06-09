using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "CaptureRocketTask", menuName = "Scriptable Objects/Tasks/CaptureRocketTask")]
public class CaptureRocketTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        ShipComponentController component = GameManager.Instance.currentGameplayManager.playerShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket });

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.MainCabin, ComponentType.Generator });

        ((MainCabinComponentController)component.componentBehaviour).onComponentPickup.AddListener(CompleteTask);

        component.Highlight(highlightMaterial, highlightColor, 1.2f, 0.5f);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.GetMainCabin().RemoveHighlight();
    }

    /*private void EvaluateComponent(ShipComponentController component)
    {
        CompleteTask();
    }*/
}