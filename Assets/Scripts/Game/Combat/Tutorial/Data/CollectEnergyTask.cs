using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "CollectEnergyTask", menuName = "Scriptable Objects/Tasks/CollectEergyTask")]
public class GatherEnergyTask : TutorialTaskSO
{
    public override void BeginTask()
    {

        GameManager.Instance.currentGameplayManager.playerShip.onEnergyChanged.AddListener(EvaluateEnergy);

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept();

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept(ComponentType.Generator);
    }

    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }

    private void EvaluateEnergy()
    {
        CompleteTask(); 
    }
}