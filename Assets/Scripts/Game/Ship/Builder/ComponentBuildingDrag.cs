using UnityEngine;
using UnityEngine.InputSystem;

public class ComponentBuildingDrag : MonoBehaviour
{
    public ShipComponentController componentPrefab;
    public bool beingDragged;
    private Camera cam;

    private void Awake() {
        cam = Camera.main;
    }

    private void Update() {
        if (beingDragged) {
            var mp = Mouse.current.position.ReadValue();

            Plane plane = new Plane(Vector3.down, Vector3.up * 0.5f);
            Ray ray = Camera.main.ScreenPointToRay(mp);
            if (plane.Raycast(ray, out float enter)) {
                Vector3 worldPosition = ray.GetPoint(enter);
                worldPosition.y = 0.01f;
                worldPosition.x = (int)(worldPosition.x);
                worldPosition.z = (int)(worldPosition.z);

                transform.position = worldPosition;
            }
        }
    }
}
