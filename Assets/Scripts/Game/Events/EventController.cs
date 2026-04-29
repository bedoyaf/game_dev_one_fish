using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventController : MonoBehaviour
{
    public List<EventData> events;
    public EventUI eventUIPrefab;
    public Canvas eventCanvas;

    private EventUI instantiatedEventUI;

    /// <summary>
    /// Display event UI from the event data
    /// TODOH - Ugly
    /// </summary>
    public void DisplayUI() {
        instantiatedEventUI = Instantiate(eventUIPrefab, eventCanvas.transform);
        var eventData = events[0];
        instantiatedEventUI.EventImage.sprite = eventData.eventImage;
        instantiatedEventUI.EventText.text = eventData.eventText;

        // Add buttons for choices
        for(int i = 0; i < eventData.choices.Count; i++) {
            var button = instantiatedEventUI.EventButtons[i];
            if (!eventData.choices[i].DoConditionsHold()) {
                button.image.color = Color.red;
            }
                int a = i;
                button.onClick.AddListener(() => OnButtonClick(a));
                button.GetComponentInChildren<TextMeshProUGUI>().text = eventData.choices[i].choiceText;
        }

        for (int i = eventData.choices.Count; i < instantiatedEventUI.EventButtons.Count; i++) {
            var button = instantiatedEventUI.EventButtons[i];
            button.gameObject.SetActive(false);
        }


    }

    public void OnButtonClick(int choice) {
        Debug.Log(choice);
        var eventData = events[0];
        eventData.choices[choice].ApplyEffects();
    }

    public void PrintEvents() {
        foreach(var ev in events) {
            ev.PrintData();
            Debug.Log("New Event");
        }
    }

    public void HideUI() {
        instantiatedEventUI.gameObject.SmartDestroy();
    }
}