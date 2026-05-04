using UnityEngine;

public class EventState : IGameState
{
    private GameplayFlowManager manager;

    public EventState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {

    }

    public void Exit()
    {

    }
}