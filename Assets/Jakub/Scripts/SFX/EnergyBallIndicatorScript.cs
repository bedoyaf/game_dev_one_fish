using UnityEngine;

public class EnergyBallIndicator : MonoBehaviour
{
    // Scale the energy quad based on capacity
    [SerializeField]
    private GeneratorComponentController generator;

    [SerializeField]
    private MeshRenderer quadIndicator;

    void Update()
    {
        quadIndicator.transform.localScale = ((float) generator.GetCurrentEnergy / generator.GetEnergyCapacity) 
            * Vector3.one;
    }
}
