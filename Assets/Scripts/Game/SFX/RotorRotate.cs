using UnityEngine;

public class RotorRotate : MonoBehaviour
{
    [SerializeField] private float speed = 180f;

    void Update()
    {
        transform.Rotate(0f, 0f, speed * MyTime.deltaTime);
    }
}
