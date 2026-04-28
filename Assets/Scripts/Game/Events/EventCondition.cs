using System;
using UnityEngine;

[Serializable]
public class EventCondition {
    public ConditionType conditionType;
    [SerializeReference]
    public EventConditionInside eventCondition;
    public enum ConditionType {
        None,
        HasParts,
        HasComponent,
    }
}

public abstract class EventConditionInside { }

[Serializable]
public class HasPartsCondition : EventConditionInside {
    public int partsCount;
}
[Serializable]
public class NoneCondition : EventConditionInside {

}
[Serializable]
public class HasComponentCondition : EventConditionInside {
    public string componentName;
    public string componentCount;
}