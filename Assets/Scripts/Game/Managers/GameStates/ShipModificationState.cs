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
    }

    public void Exit()
    {
        manager.CloseShipEditor();
        //manager.PlayerShip.RemoveControlFromEditor();  might be important idk
    }
}