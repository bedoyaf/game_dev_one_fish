using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventController : MonoBehaviour
{
    public List<EventData> events = new(); // The list of all events in the game
    private List<EventData> addedEvents = new(); // Events can add other events

    [SerializeField] private GameplayFlowManager gameplayFlowManager;
    public GameplayFlowManager GameplayManager => gameplayFlowManager;

    [Header("UI")]
    public EventUI eventUIPrefab;
    public Canvas eventCanvas;
    private EventUI instantiatedEventUI;

    private EventData currentEvent;
    private List<EventData> eventHistory = new();

    public EventData selectThisEvent;

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

        if (selectThisEvent != null) {
            currentEvent = selectThisEvent;
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

        // Prepare UI
        instantiatedEventUI = Instantiate(eventUIPrefab, eventCanvas.transform);

        // start invisible
        instantiatedEventUI.selfGroup.DOFade(0f, 0f);
        instantiatedEventUI.selfGroup.DOFade(1f, 0.1f);

        // Move, so that the pause menu is always in front
        instantiatedEventUI.transform.SetAsFirstSibling();
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
        bool changedState = currentEvent.choices[choice].ApplyEffects(this);
        HideUI();

        // If no effect changed current state
        if (!changedState) {
            gameplayFlowManager.CloseEventController();
        }
    }

    public void HideUI() {

        // Fade out -> then destroy
        instantiatedEventUI.selfGroup.DOFade(0f, 0.1f).onComplete += (
            () =>
            {
                instantiatedEventUI.gameObject.SmartDestroy();
            });
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