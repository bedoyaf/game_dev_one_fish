using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventEffect {
    public EffectType effectType;

    [SerializeReference]
    public EventEffectInside effectData;

    public bool ChangesState => effectData.ChangesState;

    public void ApplyEffect(EventController eventController) {
        effectData.ApplyEffect(eventController);
    }

    public enum EffectType {
        None,
        Fight,
        Run,
        ChangeCurrencyAmount,
        RandomChance,
        Multi,
        AddEvent,
        GetComponent,
        GetRandomComponent,
        BreakComponents,
        Repair,
    }
    // TODO - add component selection, so it is not always random component.
}
[Serializable]
public abstract class EventEffectInside {
    public abstract bool ChangesState { get; }
    public virtual Color Color => Color.gray;
    public abstract void ApplyEffect(EventController eventController);
}
public class NoneEffect : EventEffectInside {
    public override bool ChangesState => false;

    public override void ApplyEffect(EventController eventController) {
        Debug.Log("No effect from " + ToString());
    }
}

/// <summary>
/// Fight with this ship data
/// </summary>
public class FightEffect : EventEffectInside {
    [Tooltip("Select specific enemy to fight")]
    public ShipData enemy;

    [Tooltip("Should the fight be with normal or elite. Ignored is enemy ship is set.")]
    public bool elite;
    public override bool ChangesState => true;
    public override Color Color => Color.red;

    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Fight {enemy}");
        if (enemy == null) {
            eventController.GameplayManager.Fight(elite);
        }
        else {
            eventController.GameplayManager.Fight(enemy);
        }
    }
}

/// <summary>
/// Do nothing and leave
/// </summary>
public class RunEffect : EventEffectInside {
    public override bool ChangesState => false;

    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Run away");
    }
}

/// <summary>
/// Changes player's money amount
/// </summary>
public class ChangeCurrencyAmountEffect : EventEffectInside {
    public int amount;

    [Tooltip("How much more or less can the amount be")]
    public int randomness;
    public override bool ChangesState => false;
    public override Color Color => Color.gold;

    public override void ApplyEffect(EventController eventController) {
        int money = UnityEngine.Random.Range(amount - randomness, amount + randomness + 1);
        Debug.Log($"Adding/removing {money} currency");
        eventController.GameplayManager.PlayerShip.AddCurrency(money);
    }
}

/// <summary>
/// Randomly selects one effect.
/// </summary>
public class RandomChanceEffect : EventEffectInside {
    public List<ChanceData> possibilities;
    public override bool ChangesState {
        get {
            foreach (var pos in possibilities)
                if (pos.effect.ChangesState) return true;
            return false;
        }
    }

    public override Color Color => Color.yellowNice;


    public override void ApplyEffect(EventController eventController) {
        var n = UnityEngine.Random.Range(0, 100 + 1);
        Debug.Log(n);
        foreach(var pos in possibilities) {
            n -= pos.probability;
            if (n <= 0) {
                pos.effect.ApplyEffect(eventController);
            }
        }
    }
    [Serializable]
    public class ChanceData {
        [Range(0,100)]
        public int probability; // Max 100
        public EventEffect effect;
    }
}

/// <summary>
/// Simply contains multiple effects inside
/// </summary>
public class MultiEffect : EventEffectInside {
    public List<EventEffect> effects;
    public override bool ChangesState {
        get {
            foreach (var effect in effects)
                if (effect.ChangesState) return true;
            return false;
        }
    }

    public override void ApplyEffect(EventController eventController) {
        foreach (var effect in effects) {
            effect.ApplyEffect(eventController);
        }
    }
}

/// <summary>
/// Adds a new event to potential event list
/// </summary>
public class AddEventEffect : EventEffectInside {
    public EventData eventToAdd;

    public override bool ChangesState => false;
    public override Color Color => Color.lightSalmon;


    public override void ApplyEffect(EventController eventController) {
        eventController.AddEvent(eventToAdd);
    }
}

/// <summary>
/// Gives the player a component to place
/// </summary>
public class GetComponentEffect : EventEffectInside {
    public ShipComponentController component;

    public override bool ChangesState => true;

    public override void ApplyEffect(EventController eventController) {
        eventController.GameplayManager.NewComponent(component);
    }
}

/// <summary>
/// Gives the player a component to place
/// </summary>
public class GetRandomComponentEffect : EventEffectInside {
    public List<ShipComponentController> components;

    public override bool ChangesState => true;

    public override void ApplyEffect(EventController eventController) {
        eventController.GameplayManager.NewComponent(components.GetRandom());
    }
}


/// <summary>
/// Breaks certain amount of components
/// Makes sure one rocket remains
/// </summary>
public class BreakComponentsEffect : EventEffectInside {
    public int amount;
    [Tooltip("None means random")]
    public ShipComponentController.ComponentType componentType;

    public override bool ChangesState => false;

    // Destroy selected amount of components
    public override void ApplyEffect(EventController eventController) {
        var grid = eventController.GameplayManager.PlayerShip.componentGrid;
        List<ShipComponentController> components = new();
        if (componentType == ShipComponentController.ComponentType.None) {
            components = grid.GetAllNonBrokenComponents();
            
        }
        else {
            var componentTypes = grid.GetNonBrokenComponentsGroupedByType();
            if (componentTypes.ContainsKey(componentType)) {
                components = componentTypes[componentType];
            }
        }

        // Remove one rocket so that the player has something to fight with
        // Only do it for random components and if the player has enough of them
        if (componentType == ShipComponentController.ComponentType.None && components.Count > amount) {
            int index = components.FindIndex(x => x.componentType == ShipComponentController.ComponentType.Rocket);
            if (index != -1) components.RemoveAt(index);
        }

        // Remove all main cabins
        components.RemoveAll(x => x.componentType == ShipComponentController.ComponentType.MainCabin);

        for (int i = 0; i < amount && components.Count > 0; i++) {
            var comp = components.GetRandom();
            components.Remove(comp);
            comp.TakeDamage(10000000);
        }
    }
}

/// <summary>
/// Starts ship repairing
/// </summary>
public class RepairEffect : EventEffectInside {
    public override bool ChangesState => true;

    public override void ApplyEffect(EventController eventController) {
        eventController.GameplayManager.EnterRepairsMode();
    }
}

