using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouseFollowScript : MonoBehaviour
{
    [Header("Max Movement Offset")]
    [SerializeField] private Vector3 movementOffset = new Vector3(0.5f, 0.3f, 0f);

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        // Get mouse position from New Input System
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Normalize screen position so center = 0
        float normalizedX = (mousePos.x / Screen.width - 0.5f) * 2f;
        float normalizedY = (mousePos.y / Screen.height - 0.5f) * 2f;

        // Calculate positional offset
        Vector3 offset = new Vector3(
            normalizedX * movementOffset.x,
            normalizedY * movementOffset.y,
            normalizedY * movementOffset.z
        );

        Vector3 targetPosition = startPosition + offset;

        // Smooth movement
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}
