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
        HasComponents,
    }

    public bool DoesConditionHold() {
        return eventCondition.CheckCondition();
    }

    public string ReasonForFailure() {
        return eventCondition.GiveReason();
    }
}

public abstract class EventConditionInside {
    public abstract bool CheckCondition();
    public abstract string GiveReason();

}

[Serializable]
public class NoneCondition : EventConditionInside {
    public override bool CheckCondition() {
        return true;
    }

    public override string GiveReason() {
        return "";
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

    public override string GiveReason() {
        return $"You must have at least {currencyCount}$";
    }
}

/// <summary>
/// Checks if the player has x unbroken! components
/// Either choose component type or a specific component
/// If a component is chosen, the type is ignored
/// </summary>
[Serializable]
public class HasComponentsCondition : EventConditionInside {
    [Tooltip("Choose a type of components the player needs")]
    public ShipComponentController.ComponentType componentType;

    [Tooltip("Choose specific component (type will be ignored)")]
    public ShipComponentController component;

    [Tooltip("How many components the player must have (>=)")]
    public int componentCount;

    public override bool CheckCondition() {
        var ship = GameManager.Instance.currentGameplayManager.PlayerShip;
        if (component != null) {
            var componentCounts = ship.componentGrid.GetNonBrokenComponentsCountGroupedByGuid();
            if (componentCounts.ContainsKey(component.guid)) {
                return componentCounts[component.guid] >= componentCount;
            }
        }
        else {
            var componentTypes = ship.componentGrid.GetNonBrokenComponentsCountGroupedByType();
            if (componentTypes.ContainsKey(componentType)) {
                return componentTypes[componentType] >= componentCount;
            }
        }

            return false;
    }

    public override string GiveReason() {
        if (component != null) {
            return $"You must have at least {componentCount} {component.componentName}";
        }
        else {
            return $"You must have at least {componentCount} {componentType} components";
        }
    }
}