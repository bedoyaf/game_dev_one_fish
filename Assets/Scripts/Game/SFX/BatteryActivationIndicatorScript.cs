using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class BatteryActivationIndicatorScript : IndicatorScript {
    
    [SerializeField]
    private BatteryComponentController battery;

    public override void OnUpdate() {
        //quadIndicator.material.SetFloat("_Degrees", (float)battery.energyStored / battery.energyMax * 360);
        quadIndicator.material.SetFloat("_Fill", (float)battery.energyStored / battery.energyMax);
    }
}
