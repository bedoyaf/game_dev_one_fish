using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipData shipData;

    [FormerlySerializedAs("componentGrid")]
    [SerializeField] private ComponentGrid _componentGrid;
    public ComponentGrid componentGrid { get => _componentGrid; private set => _componentGrid = value; }

    private void Start() {
        //BuildShip();
        var components = componentGrid.GetAllComponents();
        foreach (var component in components) {
            component.SetShipController(this);
        }
    }

    public void BuildShip() {
        Debug.Log("Build ship called");
        if (shipData == null) return;
        DeconstructShip();
        componentGrid = shipData.BuildShip(transform);

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
        for (int i = transform.childCount - 1; i >= 0; i--) {
            transform.GetChild(i).gameObject.SmartDestroy();
        }
    }
}


