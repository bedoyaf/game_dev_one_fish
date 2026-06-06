using UnityEngine;

public class HookIndicatorScript : IndicatorScript
{
    [SerializeField]
    private MainCabinComponentController cabin;

    [SerializeField]
    private ComponentCooldown cooldown;

    public override void OnUpdate()
    {
        if (cabin == null)
            return;

        CooldownUpdate(cooldown, cabin.CanClickOnNow);
    }

}
