using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static ShipComponentController;

[CreateAssetMenu(fileName = "DamageCabinTask", menuName = "Scriptable Objects/Tasks/DamageCabinTask")]
public class DamageCabinTask : TutorialTaskSO
{
    private bool stop;

    public override void BeginTask()
    {

        var component = GameManager.Instance.currentGameplayManager.EnemyShip.GetMainCabin();

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.MainCabin });

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Rocket, ComponentType.Generator });

        component.OnDamage.AddListener(CompleteTask);

        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
    .shipComponentController.Highlight(highlightMaterial, highlightColor, 1.2f, 0.5f);

        // Idk...
        //GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MainCabinComponentController>()[0].SavePosition();
        //stop = false;
        //GameManager.Instance.StartCoroutine(FixMainCabinPosition());
    }


    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
        stop = true;
//        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<MissileComponentController>()[0]
//.shipComponentController.RemoveHighlight();
    }

}