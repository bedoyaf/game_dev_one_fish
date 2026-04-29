using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the event with choices (that have conditions and effects)
/// </summary>
[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject {
    public string eventID;
    [TextArea]
    public string eventText;
    public Sprite eventImage;
    public List<EventChoice> choices;

    [Serializable]
    public class EventChoice {
        public string choiceID;
        public string choiceText;
        public List<EventEffect> effects;
        public List<EventCondition> conditions;

        public void ApplyEffects() {
            foreach(var effect in effects) {
                effect.ApplyEffect();
            }
        }

        public bool DoConditionsHold() {
            foreach(var condition in conditions) {
                if (!condition.DoesConditionHold()) return false;
            }

            return true;
        }
    }

    public void PrintData() {
        foreach (EventChoice choice in choices) {
            Debug.Log(choice.choiceID);
            Debug.Log(choice.choiceText);

            foreach(var effect in choice.effects) {
                effect.effectData.ApplyEffect();
            }

            Debug.Log(""); 
        }
    }
}
