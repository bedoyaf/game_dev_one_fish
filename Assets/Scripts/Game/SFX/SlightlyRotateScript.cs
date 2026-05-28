using UnityEngine;

public class SlightlyRotateScript : MonoBehaviour
{
    [SerializeField] private float angle = 5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float offset = 0f;

    private Quaternion baseRotation;

    void Awake()
    {
        baseRotation = transform.localRotation;
    }

    void Update()
    {
        float rotation = Mathf.Sin(MyTime.time * speed + offset) * angle;

        transform.localRotation =
            baseRotation * Quaternion.Euler(0f, 0f, rotation);
    }
}
