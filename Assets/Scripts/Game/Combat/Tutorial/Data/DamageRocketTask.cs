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
        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
    .shipComponentController.Highlight(highlightMaterial, highlightColor, 1.2f, 0.5f);
    }
    public override void EndTask()
    {
        GameManager.Instance.SFXManager.SetFishFace(Moods.VeryHappy);
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();

    //    GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
    //.shipComponentController.RemoveHighlight();
    }
}