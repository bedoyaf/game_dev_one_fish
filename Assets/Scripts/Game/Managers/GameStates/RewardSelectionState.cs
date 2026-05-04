using UnityEngine;

public class RewardSelectionState : IGameState
{
    private GameplayFlowManager manager;

    public RewardSelectionState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.ShowRewards();
    }

    public void Exit() { }
}