using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static List<ShipComponentController> ConvertBehaviourListToComponentList<T>(List<T> behaviours) where T : BehaviourComponentControllerAbstract
    {
        List<ShipComponentController> result = new List<ShipComponentController>();

        foreach (var comp in behaviours)
        {
            result.Add(comp.shipComponentController);
        }

        return result;
    }
}
