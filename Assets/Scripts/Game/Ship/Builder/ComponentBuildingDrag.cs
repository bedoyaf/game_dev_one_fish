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

            Plane plane = new Plane(Vector3.down, Vector3.up * 1.5f);
            Ray ray = Camera.main.ScreenPointToRay(mp);
            if (plane.Raycast(ray, out float enter)) {
                Vector3 worldPosition = ray.GetPoint(enter);
                worldPosition.y -= 0.5f;
                // Now you have the world position you wanted.

                //Debug.Log($"{mp} {worldPosition}");
                //var mousePos = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, cam.transform.position.y));
                //transform.position = new Vector3(mousePos.x, transform.position.y, mousePos.z);
                transform.position = worldPosition;
            }
        }
    }
}
