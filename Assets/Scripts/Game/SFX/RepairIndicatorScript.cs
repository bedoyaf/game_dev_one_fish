using UnityEngine;

public class RepairIndicatorScript : IndicatorScript
{
    [SerializeField]
    private RepairerComponentController repairer;

    [SerializeField]
    private ComponentCooldown cooldown;

    public override void OnUpdate() {
        CooldownUpdate(cooldown, repairer.CanClickOnNow);
    }

    // Update is called once per frame
    //    void Update()
    //    {
    //        quadIndicator.transform.localScale = repairer.CanClickOnNow ? Vector3.one : Vector3.zero;
    //    }
}
