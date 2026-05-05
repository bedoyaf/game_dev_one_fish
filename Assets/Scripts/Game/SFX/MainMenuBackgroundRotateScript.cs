using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuBackgroundRotateScript : MonoBehaviour
{
    [Header("Target UI Element")]
    public RectTransform targetImage;

    [Header("Rotation Settings")]
    public float maxTiltX = 10f;
    public float maxTiltY = 10f;
    public float smoothSpeed = 5f;

    private Mouse mouse;

    void Awake()
    {
        mouse = Mouse.current;
    }

    void Update()
    {
        if (targetImage == null || mouse == null)
            return;

        Vector2 mousePos = mouse.position.ReadValue();

        // Normalize mouse position so screen center = 0
        float normalizedX = (mousePos.x / Screen.width - 0.5f) * 2f;
        float normalizedY = (mousePos.y / Screen.height - 0.5f) * 2f;

        // Calculate target rotation
        float rotX = -normalizedY * maxTiltX;
        float rotY = normalizedX * maxTiltY;

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, 0f);

        // Smooth movement
        targetImage.localRotation = Quaternion.Lerp(
            targetImage.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}
