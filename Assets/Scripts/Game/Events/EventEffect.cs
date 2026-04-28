using System;
using UnityEngine;

[Serializable]
public class EventEffect {
    public EffectType effectType;

    [SerializeReference]
    public EventEffectInside effectData;

    public enum EffectType {
        None,
        Fight,
        Run,
        LoseParts,
    }
}

public abstract class EventEffectInside {
    public virtual void ApplyEffect() { Debug.Log("No effect from " + ToString()); }
}
public class NoneEffect : EventEffectInside {
}

public class FightEffect : EventEffectInside {
    public ShipData enemy;
    public override void ApplyEffect() {
        Debug.Log($"Fight {enemy}");
    }
}

public class RunEffect : EventEffectInside {
    public override void ApplyEffect() {
        Debug.Log($"Run away");
    }
}
public class LosePartsEffect : EventEffectInside {
    public int amount;
    public override void ApplyEffect() {
        Debug.Log($"Lose {amount} parts");
    }
}
