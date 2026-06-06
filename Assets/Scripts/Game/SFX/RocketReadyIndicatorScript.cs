using Unity.VisualScripting;
using UnityEngine;

public class RocketReadyIndicatorScript : IndicatorScript
{
    [SerializeField]
    private MissileComponentController missiles;

    [SerializeField]
    private ComponentCooldown cooldown;

    [SerializeField]
    private SpriteRenderer spriteIndicator;

    public override void OnStart()
    {
        // start hidden
        spriteIndicator.transform.localScale = Vector3.zero;
    }

    public override void OnUpdate() {
        if (missiles == null)
            return;

        CooldownUpdate(cooldown, missiles.CanClickOnNow);
        spriteIndicator.transform.localScale = missiles.CanClickOnNow ? Vector3.one : Vector3.zero;
    }
}
