using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FishEyeMouseFollow : MonoBehaviour
{
    public GameObject center;

    public float maxMovementDistanceX;
    public float maxMovementDistanceZ;
    public float eyeSpeed;
    public float maxDistanceAtRatio = 0.3f;

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, CalculateTarget(), eyeSpeed * MyTime.deltaTime);
    }

    private Vector3 CalculateTarget() {
        var mousePos = Mouse.current.position.ReadValue();
        Vector2 centerInScreenSpace = Camera.main.WorldToScreenPoint(center.transform.position);
        var toMouse = mousePos - centerInScreenSpace;

        // Calculate how much to move in each direction
        var xRatio = Mathf.Abs(toMouse.x) / Screen.width;
        var zRatio = Mathf.Abs(toMouse.y) / Screen.height;
        var xMovement = maxMovementDistanceX * Mathf.Min(1, xRatio / maxDistanceAtRatio);
        var zMovement = maxMovementDistanceZ * Mathf.Min(1, zRatio / maxDistanceAtRatio);

        // Do smooth movement
        var direction = toMouse.normalized;
        var target = center.transform.position + new Vector3(direction.x * xMovement, 0, direction.y * zMovement);
        return target;
    }

    public void OnFlip() {
        transform.position = CalculateTarget();
    }
}
