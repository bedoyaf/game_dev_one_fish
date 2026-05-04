using UnityEngine;

public class WaitingForCombatState : IGameState
{
    private GameplayFlowManager manager;

    public WaitingForCombatState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.UnloadEnemy();
        manager.LoadPlayer();
    }

    public void Exit() 
    {

    }
}
