using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "DamageCabinTask", menuName = "Scriptable Objects/Tasks/DamageCabinTask")]
public class DamageCabinTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        var component = GameManager.Instance.currentGameplayManager.EnemyShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.MainCabin });

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket, ComponentType.Generator });

        component.OnDamage.AddListener(CompleteTask);

        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
.shipComponentController.Highlight(highlightMaterial, highlightColor, 1.2f, 0.5f);
    }
    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
.shipComponentController.RemoveHighlight();
    }

}