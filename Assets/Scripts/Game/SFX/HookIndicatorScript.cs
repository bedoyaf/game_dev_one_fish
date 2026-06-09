using UnityEngine;
using UnityEngine.UIElements;

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

        if (!cabin.shipComponentController.shipController.playerShip) {
            quadIndicator.transform.localScale = Vector3.zero;
            return;
        }

        if (actuallyUpdate)
            CooldownUpdate(cooldown, cabin.CanClickOnNow);
        else
            CooldownBinaryUpdate(cooldown, cabin.CanClickOnNow);
    }

}
