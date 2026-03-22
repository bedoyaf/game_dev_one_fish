using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ship editor
/// </summary>
public class ShipBuildingController : MonoBehaviour
{
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 20;
    private List<List<ShipComponentController>> components;
    [SerializeField] private ShipComponentController placeholderComponent;


    [SerializeField] List<ShipComponentController> componentPrefabs;
    [SerializeField] int selectedComponent;

    [SerializeField] ShipData shipData;

    [SerializeField] private bool placeholdersVisible;
    
    void Start()
    {
        // If new player data, generate the ship from placeholders
        components = new();
        if (shipData.components == null || shipData.components.Count == 0) {
            shipData.components = new();
            shipData.width = width;
            shipData.height = height;
            for (int i = 0; i < height; i++) {
                var row = new List<ShipComponentController>();
                components.Add(row);
                for (int j = 0; j < width; j++) {
                    var placeHolder = Instantiate(placeholderComponent, new Vector3(j, 0, i), Quaternion.identity, transform);
                    row.Add(placeHolder);
                    shipData.components.Add(null);
                }
            }
        }
        // Ship data is present, so generate the ship using it
        else {
            height = shipData.height;
            width = shipData.width;
            for (int i = 0; i < height; i++) {
                var row = new List<ShipComponentController>();
                components.Add(row);
                for (int j = 0; j < width; j++) {
                    var componentPrefab = shipData.components[i * width + j];
                    if (componentPrefab == null) {
                        componentPrefab = placeholderComponent;
                    }

                    var comp = Instantiate(componentPrefab, new Vector3(j, 0, i), Quaternion.identity, transform);
                    row.Add(comp);
                }
            }
        }

        if (placeholdersVisible) {
            placeholdersVisible = false;
            TogglePlaceholders();
        }
    }

    /// <summary>
    /// Places component
    /// </summary>
    public void OnClick(InputAction.CallbackContext context) {
        if (!context.started) return;
        RaycastAndPlaceComponent(componentPrefabs[selectedComponent]);
    }

    /// <summary>
    /// Removes component
    /// </summary>
    public void OnRightClick(InputAction.CallbackContext context) {
        if (!context.started) return;
        RaycastAndPlaceComponent(placeholderComponent, true);
    }

    /// <summary>
    /// Raycasts in the scene and tries to place the given component
    /// </summary>
    private void RaycastAndPlaceComponent(ShipComponentController componentPrefab, bool isPlaceholder = false) {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100)) {
            var component = hit.collider.gameObject.GetComponentInParent<ShipComponentController>();
            var position = component.transform.position;

            int x = (int)position.x;
            int z = (int)position.z;

            var newComponent = Instantiate(componentPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);

            components[z][x] = newComponent;
            if (isPlaceholder)
                shipData.components[z * width + x] = null;
            else
                shipData.components[z * width + x] = componentPrefab;

            Destroy(component.gameObject);
        }
    }

    /// <summary>
    /// Toggles the visibility of placeholders
    /// </summary>
    public void TogglePlaceholders() {
        placeholdersVisible = !placeholdersVisible;
        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if (shipData.components[i * width + j] != null) continue;

                if (components[i][j] != null) {
                    components[i][j].GetComponentInChildren<MeshRenderer>().enabled = placeholdersVisible;
                }
            }
        }
    }
}
