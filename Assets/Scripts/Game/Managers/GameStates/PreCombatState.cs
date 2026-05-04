using UnityEngine;

public class PreCombatState : IGameState
{
    private GameplayFlowManager manager;

    public PreCombatState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.LoadEnemy();
    }

    public void Exit() { }
}
