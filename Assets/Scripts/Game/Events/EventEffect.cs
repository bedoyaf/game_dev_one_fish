using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventEffect {
    public EffectType effectType;

    [SerializeReference]
    public EventEffectInside effectData;

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
    }
}

public abstract class EventEffectInside {
    public abstract void ApplyEffect(EventController eventController);
}
public class NoneEffect : EventEffectInside {
    public override void ApplyEffect(EventController eventController) {
        Debug.Log("No effect from " + ToString());
    }
}

public class FightEffect : EventEffectInside {
    public ShipData enemy;
    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Fight {enemy}");
    }
}

public class RunEffect : EventEffectInside {
    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Run away");
    }
}
public class ChangeCurrencyAmountEffect : EventEffectInside {
    public int amount;
    public override void ApplyEffect(EventController eventController) {
        Debug.Log($"Adding/removing {amount} currency");
        GameManager.Instance.currentGameplayManager.PlayerShip.AddCurrency(amount);
    }
}

/// <summary>
/// Randomly selects one effect.
/// </summary>
public class RandomChanceEffect : EventEffectInside {
    public List<ChanceData> possibilities;
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
        public int probability; // Max 100
        public EventEffect effect;
    }
}

/// <summary>
/// Simply contains multiple effects inside
/// </summary>
public class MultiEffect : EventEffectInside {
    public List<EventEffect> effects;
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
    public override void ApplyEffect(EventController eventController) {
        eventController.AddEvent(eventToAdd);
    }
}

// TODO
public class GetComponentEffect : EventEffectInside {
    public ShipComponentController component;
    public override void ApplyEffect(EventController eventController) {
        //eventController.AddEvent(eventToAdd);
    }
}


