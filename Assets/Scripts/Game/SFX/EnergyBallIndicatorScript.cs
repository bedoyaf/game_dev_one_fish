using UnityEngine;

public class EnergyBallIndicator : MonoBehaviour
{
    // Scale the energy quad based on capacity
    [SerializeField]
    private GeneratorComponentController generator;

    [SerializeField]
    private MeshRenderer quadIndicator;

    private static readonly float baseScale = 0.3f;

    void Update()
    {
        if (generator.GetCurrentEnergy == 0)
            quadIndicator.transform.localScale = Vector3.zero;
        else
            quadIndicator.transform.localScale = (baseScale + (1f - baseScale) * ((float)generator.GetCurrentEnergy / generator.GetEnergyCapacity))
                * Vector3.one;
    }
}
