using System;
using UnityEngine;

/// <summary>
/// Invokes event when object dies...
/// </summary>
public class DeathListener : MonoBehaviour
{
    public Action onDeath;

    private void OnDestroy() {
        onDeath?.Invoke();
    }
}
