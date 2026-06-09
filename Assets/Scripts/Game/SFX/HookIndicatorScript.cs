using UnityEngine;

public class HookIndicatorScript : IndicatorScript
{
    [SerializeField]
    private MainCabinComponentController cabin;

    [SerializeField]
    private ComponentCooldown cooldown;

    [SerializeField]
    private bool actuallyUpdate = true;

    public override void OnUpdate()
    {
        if (cabin == null)
            return;

        if (actuallyUpdate)
            CooldownUpdate(cooldown, cabin.CanClickOnNow);
        else
            CooldownBinaryUpdate(cooldown, cabin.CanClickOnNow);
    }

}
