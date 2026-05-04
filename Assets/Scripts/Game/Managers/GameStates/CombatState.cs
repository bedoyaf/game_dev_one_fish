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
        manager.EnemyShip.componentsActive = true;
        manager.EnemyShip.ResetComponentEffects();
        manager.PlayerShip.componentsActive = true;
        manager.PlayerShip.ResetComponentEffects();
    }

    public void Exit()
    {
        manager.EndCombat();
        manager.EnemyShip.componentsActive = false;
        manager.PlayerShip.componentsActive = false;
    }
}
