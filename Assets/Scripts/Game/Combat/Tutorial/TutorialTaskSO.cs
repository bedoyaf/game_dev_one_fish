using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class TutorialTaskSO : ScriptableObject
{
    public UnityEvent OnTaskCompleted;

    public TutorialStep step;

    public Image fadeScreen;

    public virtual void BeginTask() { }


    public virtual void EndTask() { }

    protected void CompleteTask()
    {
        OnTaskCompleted?.Invoke();
    }
}