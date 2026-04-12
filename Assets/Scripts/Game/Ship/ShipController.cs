using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    public ShipBuildingController shipEditor;
    public Transform componentsParent;

    /// <summary>
    /// The original ship data.
    /// </summary>
    [SerializeField] private ShipData shipData;
    private bool everythingSolid;

    /// <summary>
    /// Ship data created during battle.
    /// For now unused, might be removed
    /// </summary>
    private ShipData battleShipData;

    [FormerlySerializedAs("componentGrid")]
    [SerializeField] private ComponentGrid _componentGrid;
    public ComponentGrid componentGrid { get => _componentGrid; private set => _componentGrid = value; }

    private void Start() {
        // Create temporary ship data that will be used during game time
        //battleShipData = ScriptableObject.CreateInstance<ShipData>();
        //battleShipData = Instantiate(shipData);
        //componentGrid.ConnectGrid(battleShipData.componentGrid);

        AssignShipController();
    }

    public void BuildShip() {
        Debug.Log("Build ship called");
        if (shipData == null) return;
        DeconstructShip();
        componentGrid = shipData.BuildShip(componentsParent);

        // Needs to be here for Unity to save the ship
        #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    private void DeconstructShip()
    {
        //foreach(ShipComponentController component in shipComponents)
        //{
        //    if(component != null)DestroyImmediate(component.gameObject);
        //}
        //shipComponents.Clear();
        if (componentGrid == null) return;
        componentGrid.DestroyGrid();

        // This is just to be safe, because the above does not work after Unity restart for some reason...
        componentsParent.DestroyAllChildren();
    }

    /// <summary>
    /// Gives all components reference to this
    /// </summary>
    public void AssignShipController() {
        var components = componentGrid.GetAllComponents();
        foreach (var component in components) {
            component.SetShipController(this);
        }
    }

    public void GiveControlToEditor() {
        var components = componentGrid.GetAllComponents();

        List<ShipComponentController> prefabs = new();
        for (int i = 0; i < 3; i++) {
            prefabs.Add(components[Random.Range(0, components.Count)].componentPrefab);
        }

        shipEditor.InitializeBuilder(componentGrid, prefabs);
    }

    public void RemoveControlFromEditor() {
        shipEditor.RemoveBuilderConnection();
    }
}


