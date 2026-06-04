using UnityEngine;
using static ShipComponentController;

[CreateAssetMenu(fileName = "AdvanceTextTask", menuName = "Scriptable Objects/Tasks/AdvanceTextTask")]
public class AdvanceTextTask : TutorialTaskSO
{
    private PopupWindowsController popup;
    public override void BeginTask()
    {
        popup = step.popup.GetComponent<PopupWindowsController>();
        popup.OnSequenceCompleted.AddListener(CompleteTask);

        GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept();

        GameManager.Instance.currentGameplayManager.playerShip.DisableAllCollidersExcept();
    }

    public override void EndTask()
    {
        GameManager.Instance.currentGameplayManager.EnemyShip.EnableAllColliders();

        GameManager.Instance.currentGameplayManager.playerShip.EnableAllColliders();
    }
}