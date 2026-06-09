using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class TutorialTaskSO : ScriptableObject
{
    public UnityEvent OnTaskCompleted;

    public TutorialStep step;

    public Image fadeScreen;

    [SerializeField] protected Material highlightMaterial;
    [SerializeField] protected Color highlightColor;

    public virtual void BeginTask() { }


    public virtual void EndTask() { }

    protected void CompleteTask()
    {
        OnTaskCompleted?.Invoke();
    }
}