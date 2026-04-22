using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EnemyShipAgent : MonoBehaviour
{
    public bool thinking = false; 
    [SerializeField] private float actInterval = 1.0f;

    // How long does it take to complete one action (eg. to click a generator)
    [SerializeField] private float actActionDuraction = 0.01f;
    private float nextActTime;



    private ShipController shipController;
    //player
    [SerializeField] private ShipController playerShip;

    private List<ShipComponentController> batteries = new List<ShipComponentController>();
    private List<ShipComponentController> generators = new List<ShipComponentController>();
    private List<ShipComponentController> shields = new List<ShipComponentController>();
    private List<ShipComponentController> missiles = new List<ShipComponentController>();

    private List<ShipComponentController> playerBatteries = new List<ShipComponentController> ();
    private List<ShipComponentController> playerGenerators = new List<ShipComponentController>();
    private List<ShipComponentController> playerShields = new List<ShipComponentController>();
    private List<ShipComponentController> playerMissiles = new List<ShipComponentController>();
    private ShipComponentController playerCabin;

    void Start()
    {
        shipController= GetComponent<ShipController>();
    }

    private void SetupAllImportantSystems()
    {
        if(shipController == null)
        {
            shipController = GetComponent<ShipController>();
        }
        if(playerShip == null)
        {
            Debug.LogError("Missing player controller");
            return;
        }
        batteries = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<BatteryComponentController>());
        generators = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<GeneratorComponentController>());
        shields = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<ShieldComponentController>());
        missiles = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<MissileComponentController>());

        playerBatteries = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<BatteryComponentController>());
        playerGenerators = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<GeneratorComponentController>());
        playerShields = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<ShieldComponentController>());
        playerMissiles = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<MissileComponentController>());
    }
    //Missleading name, hope in future it fits better
    public void ActivateAgent()
    {
        SetupAllImportantSystems();
    }




    void Update()
    {
        if (Time.time >= nextActTime && thinking)
        {
            Act();
            nextActTime = Time.time + actInterval;
        }
    }

    void Act()
    {
        ActivateGenerators();
        ActivateShields();
        FireWeapons();
    }

    void ActivateGenerators()
    {
        foreach (var gen in generators)
        {
            var comp = gen.GetComponent<ShipComponentController>();

            if (!comp.activated)
            {
                comp.AgentActivateComponent();
            }
        }
    }

    void ActivateShields()
    {
        foreach (var shield in shields)
        {
            var comp = shield.GetComponent<ShipComponentController>();

            if (!comp.activated)
            {
                var target = GetWeakestComponent(shipController);
                if (target.shield != null) continue;
                comp.AgentActivateComponent( new TargetingData(target.shipComponentMeshController));
            }
        }
    }

    void FireWeapons()
    {
        var target = GetWeakestComponent(playerShip);
        if (target == null) return;

        foreach (var weapon in missiles)
        {
            weapon.AgentActivateComponent(new TargetingData(
            target.shipComponentMeshController,
            Vector3.forward //TODO Shit, make real aiming
            ));
        }
    }





    public ShipComponentController GetRandomPlayerComponent()
    {
        var comps = playerShip.componentGrid.GetAllNonBrokenComponents();
        return comps[Random.Range(0, comps.Count)];
    }

    public ShipComponentController GetWeakestComponent(ShipController ship)
    {
        var comps = ship.componentGrid.GetAllNonBrokenComponents();

        ShipComponentController weakest = null;
        float lowestHp = float.MaxValue;

        foreach (var comp in comps)
        {
          //  if (comp.broken) continue;
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
