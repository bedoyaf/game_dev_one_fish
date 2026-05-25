using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable representation of a ship's state during gameplay
/// Captures all necessary data to reconstruct the ship from save files
/// Uses only serializable types to avoid assembly conflicts
/// </summary>
[System.Serializable]
public class ShipState
{
    // Core ship identity
    public string shipDataName;

    // Resource state
    public int storedEnergy = 0;
    public int storedMoney = 0;

    // Health state
    public float mainCabinHealth = 100f;
    public float mainCabinMaxHealth = 100f;

    // Component state - tracks which components are broken
    public List<ComponentState> componentStates = new();

    public ShipState() { }
}

/// <summary>
/// Serializable state of a single ship component
/// </summary>
[System.Serializable]
public class ComponentState
{
    public int componentType;  // ShipComponentController.ComponentType as int
    public float health;
    public float maxHealth;
    public bool broken;

    public ComponentState() { }
}
