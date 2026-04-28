using System;
using System.Collections.Generic;
using UnityEngine;

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
