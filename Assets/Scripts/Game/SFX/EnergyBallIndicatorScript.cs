using Unity.VisualScripting;
using UnityEngine;

public class EnergyBallIndicator : IndicatorScript
{
    // Scale the energy quad based on capacity
    [SerializeField]
    private GeneratorComponentController generator;

    [SerializeField]
    private bool colored;

    private static readonly float baseScale = 0.3f;

    void Update()
    {
        if (colored) {
            if (generator.GetCurrentEnergy == 0)
                quadIndicator.transform.localScale = Vector3.zero;
            else
                quadIndicator.transform.localScale = (baseScale + (1f - baseScale) * ((float)generator.GetCurrentEnergy / generator.GetEnergyCapacity))
                    * Vector3.one;
        }
        else {
            //quadIndicator.transform.localScale = (baseScale + (1f - baseScale)) * Mathf.Max(((float)generator.GetCurrentEnergy+1) / generator.GetEnergyCapacity, 1.0f)
            //        * Vector3.one;
            quadIndicator.material.SetFloat("_Degrees", generator.EnergyBuffer * 360);

        }
    }
}
