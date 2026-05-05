using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the event with choices (that have conditions and effects)
/// </summary>
[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject {
    public string eventID;
    public EventType eventType;
    public Sprite eventImage;

    [Tooltip("Minimum map stage at which this event can occur")]
    public int minimumStage;

    [TextArea]
    public string eventText;

    public List<EventChoice> choices;

    [Serializable]
    public class EventChoice {
        public string choiceID;
        public string choiceText;
        public List<EventEffect> effects;
        public List<EventCondition> conditions;

        /// <summary>
        /// Applies effects and returns whether there was game changing state
        /// </summary>
        /// <param name="eventController"></param>
        /// <returns></returns>
        public bool ApplyEffects(EventController eventController) {
            bool changedState = false;
            foreach(var effect in effects) {
                effect.ApplyEffect(eventController);
                changedState = changedState || effect.ChangesState;
            }
            return changedState;
        }

        public bool DoConditionsHold() {
            foreach(var condition in conditions) {
                if (!condition.DoesConditionHold()) return false;
            }

            return true;
        }
    }

    public enum EventType {
        Positive,
        Negative,
        Challenge,
    }

    //public void PrintData() {
    //    foreach (EventChoice choice in choices) {
    //        Debug.Log(choice.choiceID);
    //        Debug.Log(choice.choiceText);

    //        foreach(var effect in choice.effects) {
    //            effect.effectData.ApplyEffect();
    //        }

    //        Debug.Log(""); 
    //    }
    //}
}
