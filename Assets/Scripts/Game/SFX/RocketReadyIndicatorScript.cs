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

    private Vector3 localScale = new Vector3(0.37f, 0.37f, 0.37f);

    public override void OnStart()
    {
        // start hidden
        // localScale = transform.localScale;
        spriteIndicator.transform.localScale = Vector3.zero;
    }

    public override void OnUpdate() {
        if (missiles == null)
            return;

        CooldownUpdate(cooldown, missiles.CanClickOnNow);
        spriteIndicator.transform.localScale = missiles.CanClickOnNow ? localScale : Vector3.zero;
    }
}
