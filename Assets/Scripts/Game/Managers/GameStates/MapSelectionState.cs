using UnityEngine;

public class MapSelectionState : IGameState
{
    private GameplayFlowManager manager;

    public MapSelectionState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.OpenMapController();
        manager.sfx.Lightspeed(true);
    }

    public void Exit()
    {
        manager.sfx.Lightspeed(false);
    }
}