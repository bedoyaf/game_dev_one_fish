using UnityEngine;

public class GameEndingState : IGameState
{
    private GameplayFlowManager manager;

    public GameEndingState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.PlayGameEndingCutscene();
    }

    public void Exit() 
    {

    }
}
