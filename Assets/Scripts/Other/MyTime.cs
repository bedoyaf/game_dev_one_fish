using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A global time class
/// Runs update before anyone else
/// </summary>
[DefaultExecutionOrder(-1)]
public class MyTime : SmartSingleton<MyTime>
{
    public static float deltaTime;
    
    // Split for special effects (still active even when paused)
    public static float slowDownOverride = 1;
    // Split because _timeScale weirdness
    public static float pausedOverride = 1;

    public static float timeScale;
    public static float time;
    private static SortedList<float, GameObject> destructionQueue = new();
    [SerializeField] private float _timeScale = 1;

    [Header("Debug stuff")]
    // For showing in the inspector
    [SerializeField] private float _deltaTime;
    [SerializeField] private float _time;
    [SerializeField] private float _realTime;
    [SerializeField] private float _timeDiff;
    [SerializeField] private List<float> _destructionQueueKeys;
    [SerializeField] private List<GameObject> _destructionQueueValues;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        // Create the black scene transition instance
        var obj = new GameObject("MyTime[Persistent]");
        obj.AddComponent<MyTime>();
        DontDestroyOnLoad(obj);
    }

    /// <summary>
    /// Something like gameobject.Destroy(duration)
    /// </summary>
    /// <param name="obj">The object to destroy</param>
    /// <param name="duration">After how long (in seconds)</param>
    public static void ScheduleDestruction(GameObject obj, float duration) {
        destructionQueue.Add(time + duration, obj);
    }

    void Update()
    {
        // Not accurate in the long run, but I don't think it matters to us.
        deltaTime = Time.deltaTime * Mathf.Min(pausedOverride, Mathf.Min(slowDownOverride, timeScale));
        time += deltaTime;

        // WHAT ? 
        // Modifiable from the inspector
        timeScale = _timeScale;

        // Debug only
        _deltaTime = deltaTime;
        _time = time;
        _realTime = Time.time;
        _timeDiff = Mathf.Abs(_realTime - _time);
        _destructionQueueKeys = destructionQueue.Keys.ToList();
        _destructionQueueValues = destructionQueue.Values.ToList();

        // Find how many objects from destruction queue are up for destruction
        int n = 0;
        var keys = destructionQueue.Keys;
        while(n < destructionQueue.Count) {
            if (keys[n] > time) {
                break;
            }
            n++;
        }

        // Destroy the game objects
        var values = destructionQueue.Values;
        for (int i = 0; i < n; i++) {
            if (values[i] != null) 
                Destroy(values[i]);
        }

        for (int i = 0; i < n; i++) {
            destructionQueue.RemoveAt(0);
        }
    }

    /// <summary>
    /// Own version of WaitForSeconds
    /// </summary>
    /// <param name="seconds"></param>
    public static IEnumerator WaitForSeconds(float seconds) {
        float endTime = time + seconds;
        while(time < endTime) {
            yield return null;
        }
    }
}
