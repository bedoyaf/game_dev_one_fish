using UnityEngine;

public class CombatState : IGameState
{
    private GameplayFlowManager manager;

    public CombatState(GameplayFlowManager manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
            manager.EnterCombat();
    }

    public void Exit()
    {
            manager.EndCombat();
    }
}
