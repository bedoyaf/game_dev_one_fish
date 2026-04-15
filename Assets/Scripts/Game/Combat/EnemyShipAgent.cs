using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EnemyShipAgent : MonoBehaviour
{
    private ShipController shipController;
    [SerializeField] private ShipController playerShip;

    private List<ShipComponentController> enemeyBatteries = new List<ShipComponentController> ();
    private List<ShipComponentController> enemyGenerators = new List<ShipComponentController>();
    private List<ShipComponentController> enemyShields = new List<ShipComponentController>();
    private List<ShipComponentController> enemyGuns = new List<ShipComponentController>();
    private ShipComponentController enemyCabin;

    void Start()
    {
        shipController= GetComponent<ShipController>();
    }

    private void GetAllImportantPlayerSystems()
    {

    }

    void Update()
    {

    }

    void Act()
    {
        var weapons = shipController.componentGrid.GetComponentsOfType<MissileComponentController>(false);

        if (weapons.Count == 0)
            return;

        var weapon = weapons[0];

        var target = GetWeakestEnemyComponent();

        if (target == null)
            return;

        weapon.OnTargetSelected(new TargetingData(
            target.GetComponentInChildren<ShipComponentMeshController>(),
            Vector3.forward // Shit
        ));
    }


    public ShipComponentController GetRandomEnemyComponent()
    {
        var comps = playerShip.componentGrid.GetAllNonBrokenComponents();
        return comps[Random.Range(0, comps.Count)];
    }

    public ShipComponentController GetWeakestEnemyComponent()
    {
        var comps = playerShip.componentGrid.GetAllNonBrokenComponents();

        ShipComponentController weakest = null;
        float lowestHp = float.MaxValue;

        foreach (var comp in comps)
        {
            if (comp.health < lowestHp)
            {
                lowestHp = comp.health;
                weakest = comp;
            }
        }

        return weakest;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;

        float width = 150;
        float height = 50;

        float x = 10;
        float y = Screen.height - height - 10;

        if (GUI.Button(new Rect(x, y, width, height), "ENEMY ACT", style))
        {
            Act();
        }
    }
}
