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

        // only outside the tutorial I think...
        if(!manager.tutorialRunning)
            manager.sfx.SetDayTime(true);
    }

    public void Exit() { }
}
