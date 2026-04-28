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
    public ShipController playerShip;

    private EventUI instantiatedEventUI;

    /// <summary>
    /// Display event UI from the event data
    /// TODOH - Ugly
    /// </summary>
    public void DisplayUI() {
        instantiatedEventUI = Instantiate(eventUIPrefab, eventCanvas.transform);
        var eventData = events[0];
        instantiatedEventUI.EventText.text = eventData.eventText;
        for(int i = 0; i < instantiatedEventUI.EventButtons.Count; i++) {
            var button = instantiatedEventUI.EventButtons[i];
            if (i < eventData.choices.Count) {
                int a = i;
                button.onClick.AddListener(() => OnButtonClick(a));
                button.GetComponentInChildren<TextMeshProUGUI>().text = eventData.choices[i].choiceText;
            }
            else {
                button.gameObject.SetActive(false);
            }
        }
        instantiatedEventUI.EventImage.sprite = eventData.eventImage;
    }

    public void OnButtonClick(int choice) {
        Debug.Log(choice);
        var eventData = events[0];
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