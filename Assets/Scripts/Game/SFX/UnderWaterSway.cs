using UnityEngine;

public class UnderWaterSway : MonoBehaviour
{

    [Header("Movement")]
    public float verticalAmplitude = 0.15f;
    public float verticalSpeed = 1.2f;

    [Header("Rotation")]
    public float swayAngle = 8f;
    public float swaySpeed = 0.8f;

    [Header("Offset")]
    public float randomOffset = 0f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;

        if (randomOffset == 0f)
            randomOffset = Random.Range(0f, 100f);
    }

    void OnEnable()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float t = MyTime.time;

        float bob = Mathf.Sin((t + randomOffset) * verticalSpeed) * verticalAmplitude;
        float sway = Mathf.Sin((t + randomOffset) * swaySpeed) * swayAngle;

        transform.localPosition = startPos + new Vector3(0f, bob, 0f);
        transform.localRotation = Quaternion.Euler(0f, 0f, sway);
    }

}
