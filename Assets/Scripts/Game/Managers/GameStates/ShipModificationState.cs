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

        // remove the visuals for the picked up components
        manager.combatController.ClearInventory();
    }

    public void Exit()
    {
        manager.CloseShipEditor();
        manager.gameUi.HideSkipButton();
        // clear the inventory now !!
        manager.rewardController.ClearStoredComponents();
        //manager.PlayerShip.RemoveControlFromEditor();  might be important idk
    }
}