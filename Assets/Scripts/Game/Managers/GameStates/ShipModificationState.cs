using UnityEngine;

public class ShipModificationState : IGameState
{
    private GameplayFlowManager manager;

    public ShipModificationState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.OpenShipBuilder();
        manager.gameUi.ShowSkipButton();
    }

    public void Exit()
    {
        manager.CloseShipEditor();
        manager.gameUi.HideSkipButton();
        //manager.PlayerShip.RemoveControlFromEditor();  might be important idk
    }
}