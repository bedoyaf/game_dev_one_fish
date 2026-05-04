using System;
using UnityEngine;

[Serializable]
public class EventCondition {
    public ConditionType conditionType;
    [SerializeReference]
    public EventConditionInside eventCondition;
    public enum ConditionType {
        None,
        HasCurrency,
        HasComponent,
    }

    public bool DoesConditionHold() {
        return eventCondition.CheckCondition();
    }
}

public abstract class EventConditionInside {
    public abstract bool CheckCondition();
}

[Serializable]
public class NoneCondition : EventConditionInside {
    public override bool CheckCondition() {
        return true;
    }
}

/// <summary>
/// Check if player has enough money
/// </summary>
[Serializable]
public class HasCurrencyCondition : EventConditionInside {
    public int currencyCount;

    public override bool CheckCondition() {
        return GameManager.Instance.currentGameplayManager.PlayerShip.storedMoney >= currencyCount;
    }
}

/// <summary>
/// Checks if the player has x unbroken! components
/// </summary>
[Serializable]
public class HasComponentCondition : EventConditionInside {
    public ShipComponentController component;
    public int componentCount;

    public override bool CheckCondition() {
        var ship = GameManager.Instance.currentGameplayManager.PlayerShip;
        var componentCounts = ship.componentGrid.GetNonBrokenComponentsCountGroupedByGuid();
        if (componentCounts.ContainsKey(component.guid)) {
            return componentCounts[component.guid] >= componentCount;
        }
        return false;
    }
}