using UnityEngine;

public class RepairState : IGameState
{
    private GameplayFlowManager manager;

    public RepairState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.mouseController.EnterRepairsMode();
        manager.gameUi.ShowRepairsButton();
    }

    public void Exit()
    {
        manager.mouseController.ExitRepairsMode();
        manager.gameUi.HideRepairsButton();
    }
}
