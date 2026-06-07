using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Only follows the mouse around the screen.
/// </summary>
public class ComponentBuildingDrag : MonoBehaviour
{
    /// <summary>
    /// Stores a reference to prefab, which it represents
    /// </summary>
    public ShipComponentController componentPrefab;

    /// <summary>
    /// If this is being dragged, this is a reference to the original object from which this was instantiated
    /// </summary>
    public ComponentBuildingDrag originalObject;
    public bool beingDragged;
    public GameObject outline;
    private Camera cam;
    private Vector3 builderOffsetFromGrid;

    private void Awake() {
        cam = Camera.main;
    }

    public void Setup(Transform builderTransform, ComponentBuildingDrag original) {
        beingDragged = true;
        originalObject = original;
        var pos = builderTransform.position;
        builderOffsetFromGrid = pos - new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
        GetComponentInChildren<Collider>().enabled = false;
    }

    /// <summary>
    /// Makes the component follow the mouse
    /// </summary>
    private void Update() {
        if (beingDragged) {
            var mp = Mouse.current.position.ReadValue();

            Plane plane = new Plane(Vector3.down, Vector3.up * 0.5f);
            Ray ray = Camera.main.ScreenPointToRay(mp);
            if (plane.Raycast(ray, out float enter)) {
                Vector3 worldPosition = ray.GetPoint(enter) - builderOffsetFromGrid;
                worldPosition.y = 0.01f;
                worldPosition.x = (int)(worldPosition.x);
                worldPosition.z = (int)(worldPosition.z);
                //if (worldPosition.x < 0) worldPosition.x -= 1;
                //if (worldPosition.z < 0) worldPosition.z -= 1;

                transform.position = worldPosition + builderOffsetFromGrid;
            }
        }
    }
}
