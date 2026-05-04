using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventController : MonoBehaviour
{
    public List<EventData> events = new();
    private List<EventData> addedEvents = new();
    public EventUI eventUIPrefab;
    public Canvas eventCanvas;

    private EventUI instantiatedEventUI;
    private EventData currentEvent;
    private List<EventData> eventHistory = new();

    /// <summary>
    /// Selects event and shows it on the screen
    /// </summary>
    public void NextEvent() {
        if (addedEvents.Count > 0 && Random.Range(0, 3) == 0) {
            SelectRandomEvent(addedEvents);
        }
        else {
            SelectRandomEvent(events);
        }

        DisplayUI(currentEvent);
    }

    /// <summary>
    /// Selects random event from the given list and stores it into history.
    /// </summary>
    /// <param name="events"></param>
    private void SelectRandomEvent(List<EventData> events) {
        int index = Random.Range(0, events.Count);
        currentEvent = events[index];
        events.RemoveAt(index);
        eventHistory.Add(currentEvent);
    }

    /// <summary>
    /// Display event UI from the event data
    /// TODOH - Ugly
    /// </summary>
    public void DisplayUI(EventData eventData = null) {
        if (eventData == null) {
            eventData = currentEvent == null ? events[0] : currentEvent;
            currentEvent = eventData;
        }

        instantiatedEventUI = Instantiate(eventUIPrefab, eventCanvas.transform);
        instantiatedEventUI.EventImage.sprite = eventData.eventImage;
        instantiatedEventUI.EventText.text = eventData.eventText;

        // Add buttons for choices
        for(int i = 0; i < eventData.choices.Count; i++) {
            var button = instantiatedEventUI.EventButtons[i];
            if (!eventData.choices[i].DoConditionsHold()) {
                button.image.color = Color.red;
            }
            else {
                int a = i; // Avoiding variable transfer for lambdas
                button.onClick.AddListener(() => OnButtonClick(a));
            }
            button.GetComponentInChildren<TextMeshProUGUI>().text = eventData.choices[i].choiceText;
        }

        // Hide rest of the buttons
        for (int i = eventData.choices.Count; i < instantiatedEventUI.EventButtons.Count; i++) {
            var button = instantiatedEventUI.EventButtons[i];
            button.gameObject.SetActive(false);
        }
    }

    public void OnButtonClick(int choice) {
        currentEvent.choices[choice].ApplyEffects(this);
        HideUI();

        // If no effect changed current state, 
        if (GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey == GameplayFlowManager.GameStates.Event) {
            GameManager.Instance.currentGameplayManager.EventDone();
        }
    }

    public void HideUI() {
        instantiatedEventUI.gameObject.SmartDestroy();
    }

    public void AddEvent(EventData eventData) {
        addedEvents.Add(eventData);
    }

    //public void PrintEvents() {
    //    foreach (var ev in events) {
    //        ev.PrintData();
    //        Debug.Log("New Event");
    //    }
    //}
}