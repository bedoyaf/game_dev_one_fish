using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipData shipData;

    public void BuildShip() {
        if (shipData == null) return;

        shipData.BuildShip(transform.position, transform);
    }
}


