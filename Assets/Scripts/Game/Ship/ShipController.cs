using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipData shipData;
    private List<ShipComponentController> shipComponents = new List<ShipComponentController>();
    public void BuildShip() {
        if (shipData == null) return;
        DeconstructShip();
        shipComponents = shipData.BuildShip(transform.position, transform);
    }

    private void DeconstructShip()
    {
        foreach(ShipComponentController component in shipComponents)
        {
            if(component != null)DestroyImmediate(component.gameObject);
        }
        shipComponents.Clear();
    }
}


