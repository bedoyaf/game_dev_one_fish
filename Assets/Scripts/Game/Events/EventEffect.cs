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
    }
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
    public ShipData enemy;
    public override bool ChangesState => true;
    public override Color Color => Color.red;

    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Fight {enemy}");
        eventController.GameplayManager.Fight(enemy);
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
    public override bool ChangesState => false;
    public override Color Color => Color.gold;

    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Adding/removing {amount} currency");
        eventController.GameplayManager.PlayerShip.AddCurrency(amount);
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
/// Gives the player x component repairs
/// </summary>
public class RepairEffect : EventEffectInside {
    public int repairAmount;

    public override bool ChangesState => true;

    public override void ApplyEffect(EventController eventController) {
        //eventController.GameplayManager.NewComponent(components.GetRandom());
    }
}



