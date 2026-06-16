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
        GameManager.Instance.SFXManager.SetFishFace(Moods.JustHappy, false);
        manager.EnterCombat();
        
        manager.EnemyShip.componentsActive = true;
        manager.EnemyShip.ResetComponentEffects();
        
        // TODO: enemy thinking true

        manager.PlayerShip.componentsActive = true;
        manager.PlayerShip.ResetComponentEffects();

        

    }

    public void Exit()
    {
        manager.EndCombat();
        
        manager.EnemyShip.componentsActive = false;

        // TODO: enemy thinking false

        manager.PlayerShip.componentsActive = false;
        manager.PlayerShip.ResetComponentEffects();

        // If was doing some action (like aiming a rocket or a shield) -> reset on combat end
        manager.mouseController.Reset();

        manager.sfx.SetDayTime(false, true);
    }
}
