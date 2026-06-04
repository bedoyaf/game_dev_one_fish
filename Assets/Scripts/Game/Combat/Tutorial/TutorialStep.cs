using System;
using UnityEngine;

[Serializable]

public class TutorialStep
{
    [Tooltip("The visual popup")]
    public GameObject popup;

    [Tooltip("The logic needed")]
    public TutorialTaskSO task;
}