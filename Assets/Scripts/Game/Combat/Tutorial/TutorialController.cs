using System;
using System.Collections.Generic;
using UnityEngine;
using static ShipComponentController; // Z tvého kódu

public class TutorialController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject tutorialRoot;

    [Header("Tutorial Flow")]
    [SerializeField] private TutorialStep[] tutorialSteps;

    [Header("Settings")]
    [SerializeField] public List<ComponentType> typesToDestroyForTutorial;

    public bool IsRunning { get; private set; }
    private int currentStepIndex;

    private void Start()
    {
        tutorialRoot.SetActive(false);
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Length == 0) return;

        IsRunning = true;
        currentStepIndex = 0;
        tutorialRoot.SetActive(true);

        ShowCurrentStep();
    }

    public void EndTutorial()
    {
        IsRunning = false;

        DisconnectCurrentTask();

        // Vypneme všechny vizuály
        foreach (var step in tutorialSteps)
        {
            if (step.popup != null) step.popup.SetActive(false);
        }

        tutorialRoot.SetActive(false);
    }

    public void NextStep()
    {
        if (!IsRunning) return;

        DisconnectCurrentTask();

        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Length)
        {
            EndTutorial();
            GameManager.Instance.currentGameplayManager.OnRegularTutorialEnd();
            return;
        }

        ShowCurrentStep();
    }

    public void PreviousStep()
    {
        if (!IsRunning) return;

        DisconnectCurrentTask();
        currentStepIndex = Mathf.Max(0, currentStepIndex - 1);
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        foreach (var step in tutorialSteps)
        {
            if (step.popup != null) step.popup.SetActive(false);
        }

        TutorialStep currentStep = tutorialSteps[currentStepIndex];

        if (currentStep.popup != null) currentStep.popup.SetActive(true);

        if (currentStep.task != null)
        {
            currentStep.task.OnTaskCompleted.AddListener(NextStep);
            currentStep.task.step = currentStep;
            currentStep.task.BeginTask();
        }
    }

    private void DisconnectCurrentTask()
    {
        if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Length)
        {
            TutorialTaskSO activeTask = tutorialSteps[currentStepIndex].task;
            if (activeTask != null)
            {
                activeTask.OnTaskCompleted.RemoveListener(NextStep);
                activeTask.EndTask();
            }
        }
    }
}