using UnityEngine;

public class HeartBeatScriptIndicator : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float scaleAmount = 0.1f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }


    void Update()
    {
        float pulse = 1f + Mathf.Sin(MyTime.time * speed) * scaleAmount;
        transform.localScale = baseScale * pulse;
    }
}
