using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewComponentGenerator", menuName = "Scriptable Objects/Generation/Component Generator")]
public class ComponentGeneratorSO : ScriptableObject
{
    [Serializable]
    public struct GuaranteedComponent
    {
        public ShipComponentController componentPrefab;
        [Min(1)] public int count;
    }

    [Serializable]
    public struct RandomComponentEntry
    {
        public ShipComponentController componentPrefab;
        [Range(1, 100)] public int weight;
    }

    [Header("Generation Limits")]
    public int maxTotalComponents = 10;

    [Header("Pools")]
    public List<GuaranteedComponent> guaranteedComponents = new List<GuaranteedComponent>();
    public List<RandomComponentEntry> randomPool = new List<RandomComponentEntry>();

    public List<ShipComponentController> GenerateComponentList()
    {
        List<ShipComponentController> generatedList = new List<ShipComponentController>();

        // Phase 1: Add guaranteed components
        foreach (var guaranteed in guaranteedComponents)
        {
            for (int i = 0; i < guaranteed.count; i++)
            {
                if (generatedList.Count < maxTotalComponents)
                {
                    generatedList.Add(guaranteed.componentPrefab);
                }
                else
                {
                    Debug.LogWarning("ComponentGenerator reached maximum capacity just from guaranteed components.");
                    return generatedList;
                }
            }
        }

        // Phase 2: Fill the remaining slots from the random pool based on weight
        if (randomPool.Count > 0)
        {
            int totalWeight = 0;
            foreach (var entry in randomPool)
            {
                totalWeight += entry.weight;
            }

            while (generatedList.Count < maxTotalComponents)
            {
                int randomValue = UnityEngine.Random.Range(0, totalWeight);
                int currentWeight = 0;

                foreach (var entry in randomPool)
                {
                    currentWeight += entry.weight;
                    if (randomValue < currentWeight)
                    {
                        generatedList.Add(entry.componentPrefab);
                        break;
                    }
                }
            }
        }

        // Phase 3: Shuffle the list so guaranteed items aren't always first
        ShuffleList(generatedList);

        return generatedList;
    }

    private void ShuffleList(List<ShipComponentController> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + UnityEngine.Random.Range(0, n - i);
            ShipComponentController temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }
}