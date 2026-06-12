using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "CollectEnergyTask", menuName = "Scriptable Objects/Tasks/CollectEergyTask")]
public class GatherEnergyTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        GameManager.Instance.currentGameplayManager.playerShip.onEnergyCollected.AddListener(EvaluateEnergy);

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept();

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(new ComponentType[] { ComponentType.Generator });

        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<GeneratorComponentController>()[0]
            .shipComponentController.Highlight(highlightMaterial, highlightColor, 1.2f, 0.5f);
    }

    public override void EndTask()
    {
        GameManager.Instance.SFXManager.SetFishFace(Moods.FeelsGood);
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.componentGrid.GetComponentsOfType<GeneratorComponentController>()[0]
    .shipComponentController.RemoveHighlight();
    }

    private void EvaluateEnergy()
    {
        CompleteTask(); 
    }
}