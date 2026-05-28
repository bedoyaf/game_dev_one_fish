using UnityEngine;

public class EyesMoveScript : MonoBehaviour
{
    [SerializeField] private float distance = 0.006f;
    [SerializeField] private float speed = 5f;

    private Vector3 startPos;

    void Awake()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(MyTime.time * speed) * distance;

        transform.localPosition =
            startPos + new Vector3(x, 0f, 0f);
    }
}
