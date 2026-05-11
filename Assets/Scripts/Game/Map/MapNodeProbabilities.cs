using System;
using UnityEngine;

/// <summary>
/// data structure for easy storage of the probabilities for the map
/// </summary>
[Serializable]
public class MapNodeProbabilities
{
    [Range(0, 1)] public float combatProbability = 0.6f;
    [Range(0, 1)] public float eventProbability = 0.2f;
    [Range(0, 1)] public float eliteProbability = 0.15f;
    [Range(0, 1)] public float restProbability = 0.05f;

    // Ensure probabilities sum to 1
    public void ValidateProbabilities()
    {
        float total = combatProbability + eventProbability + eliteProbability + restProbability;
        if (Math.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning("Probabilities should sum to 1. Normalizing values.");
            combatProbability /= total;
            eventProbability /= total;
            eliteProbability /= total;
            restProbability /= total;
        }
    }

    public float[] GetProbabilities()
    {
        return new float[] { combatProbability, eventProbability, eliteProbability, restProbability };
    }
}
