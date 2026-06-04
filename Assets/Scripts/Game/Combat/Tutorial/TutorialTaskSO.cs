using UnityEngine;
using UnityEngine.Events;

public abstract class TutorialTaskSO : ScriptableObject
{
    public UnityEvent OnTaskCompleted;

    public TutorialStep step;

    public virtual void BeginTask() { }


    public virtual void EndTask() { }

    protected void CompleteTask()
    {
        OnTaskCompleted?.Invoke();
    }
}