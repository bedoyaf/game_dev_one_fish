using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipAgent : MonoBehaviour
{

    public bool thinking = false; 
    [SerializeField] private float actInterval = 1.0f;
    private float nextActTime;

    [SerializeField] private List<AgentBehavior> cycleBehaviors;
    private int currentBehaviorIndex = 0;
    private AgentBehavior CurrentBehavior => cycleBehaviors.Count > 0 ? 
        cycleBehaviors[currentBehaviorIndex] : AgentBehavior.EnergyCollection;

    // How often switch between behaviors
    [SerializeField] private float behaviorSwitchInterval = 2.5f;
    private float nextBehaviorSwitchTime;

    // How long does it take to complete one action (eg. to click a generator)
    [SerializeField] private float actActionDuraction = 0.01f;


    private ShipController shipController;
    //player
    [SerializeField] private ShipController playerShip;

    private List<ShipComponentController> batteries = new List<ShipComponentController>();
    private List<ShipComponentController> generators = new List<ShipComponentController>();
    private List<ShipComponentController> shields = new List<ShipComponentController>();
    private List<ShipComponentController> missiles = new List<ShipComponentController>();
    private List<ShipComponentController> repairers = new List<ShipComponentController>();

    private List<ShipComponentController> playerBatteries = new List<ShipComponentController> ();
    private List<ShipComponentController> playerGenerators = new List<ShipComponentController>();
    private List<ShipComponentController> playerShields = new List<ShipComponentController>();
    private List<ShipComponentController> playerMissiles = new List<ShipComponentController>();
    private ShipComponentController playerCabin;
    private ShipComponentController ownCabin;


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
        repairers = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<RepairerComponentController>());


        // Assume cabin always exists (kinda has to)
        ownCabin = Utils.ConvertBehaviourListToComponentList(shipController.componentGrid.GetComponentsOfType<MainCabinComponentController>())[0];


        playerBatteries = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<BatteryComponentController>());
        playerGenerators = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<GeneratorComponentController>());
        playerShields = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<ShieldComponentController>());
        playerMissiles = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<MissileComponentController>());

        // Assume cabin always exists (kinda has to)
        playerCabin = Utils.ConvertBehaviourListToComponentList(playerShip.componentGrid.GetComponentsOfType<MainCabinComponentController>())[0];

    }
    //Missleading name, hope in future it fits better
    public void ActivateAgent()
    {
        SetupAllImportantSystems();
    }


    enum AgentBehavior
    {
        EnergyCollection,
        Shielding,
        Attacking,

        ShieldCabin,
        AttackCabin,

        Repairing
    }

    void Update()
    {
        if (MyTime.time >= nextBehaviorSwitchTime && thinking)
        {
            CycleBehavior();
            nextBehaviorSwitchTime = MyTime.time + behaviorSwitchInterval;
        }

        if (MyTime.time >= nextActTime && thinking)
        {
            Act();
            nextActTime = MyTime.time + actActionDuraction;
        }
    }

    void CycleBehavior()
    {
        currentBehaviorIndex++;
        currentBehaviorIndex %= (int) Mathf.Max(1, cycleBehaviors.Count);
    }

    void Act()
    {
        // NOTE: maybe a better way than a switch like through objects / scripts
        //       but honestly this might be good enough
        switch (CurrentBehavior)
        {
            case AgentBehavior.EnergyCollection:
                ActivateGenerator();
                break;
            case AgentBehavior.Shielding:
                ActivateShield();
                break;
            case AgentBehavior.Attacking:
                FireWeapon();
                break;
            case AgentBehavior.ShieldCabin:
                ActivateShieldCabin();
                break;
            case AgentBehavior.AttackCabin:
                FireWeaponAtCabin();
                break;
            case AgentBehavior.Repairing:
                RepairComponent();
                break;
            default:
                break;
        }

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

    void ActivateGenerator()
    {
        // find the first one that can be gathered
        // TODO: maybe some randomness / cleverness
        foreach (var gen in generators)
        {
            var comp = gen.GetComponent<ShipComponentController>();

            if (!comp.broken && !comp.activated)
            {
                comp.AgentActivateComponent();
                return;
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

    void ActivateShieldCabin()
    {
        // just put all shields on own cabin
        foreach (var shield in shields)
        {
            var comp = shield.GetComponent<ShipComponentController>();

            if (!comp.broken && !comp.activated)
            {
                var target = ownCabin;
                if (target.shield != null) continue;
                comp.AgentActivateComponent(new TargetingData(target.shipComponentMeshController));
            }
        }
    }

    void ActivateShield()
    {
        foreach (var shield in shields)
        {
            var comp = shield.GetComponent<ShipComponentController>();

            // activate the first one that can be
            if (!comp.broken && !comp.activated)
            {
                var target = GetWeakestComponent(shipController);
                if (target.shield != null) continue;
                comp.AgentActivateComponent(new TargetingData(target.shipComponentMeshController));
                return;
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

    void FireWeapon()
    {
        var target = GetWeakestComponent(playerShip);
        if (target == null) return;

        foreach (var weapon in missiles)
        {
            // BROKEN really important here!!!
            if (!weapon.broken && !weapon.activated)
            {

                // pick a random direction
                var dir = MouseController.ENEMY_DIRECTIONS[Random.Range(0, 2)];

                // Pick random tile of the component
                var randomOffset = SelectRandomComponentTile(target);

                // Hard put one direction for testing
                // dir = MouseController.ENEMY_DIRECTIONS[0];
                weapon.AgentActivateComponent(new TargetingData(
                target.shipComponentMeshController,
                dir,
                randomOffset
                ));

                return;
            }
        }
    }


    void FireWeaponAtCabin()
    {
        var target = playerCabin;
        if (target == null) return;

        foreach (var weapon in missiles)
        {
            // BROKEN really important here!!!
            if (!weapon.broken && !weapon.activated)
            {

                // pick a random direction
                var dir = MouseController.ENEMY_DIRECTIONS[Random.Range(0, 2)];

                // Hard put one direction for testing
                // dir = MouseController.ENEMY_DIRECTIONS[0];

                weapon.AgentActivateComponent(new TargetingData(
                target.shipComponentMeshController,
                dir
                ));

                return;
            }
        }
    }

    void RepairComponent()
    {
        // Find a broken component and fix it
        if (repairers.Count <= 0)
            CycleBehavior();

        foreach(var comp in shipController.componentGrid.GetAllComponents())
        {
            if(comp.broken)
            {
                // find a repairer that works
                foreach (var rep in repairers)
                {
                    if(!rep.broken && !rep.activated)
                    {
                        rep.AgentActivateComponent(new TargetingData(
                            comp.shipComponentMeshController));

                        return;
                    }

                }

                return;
            }
        }

        // if here -> no broken -> cycle
        CycleBehavior();
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

    /// <summary>
    /// Returns offset to use for targeting purposes
    /// </summary>
    public Vector3 SelectRandomComponentTile(ShipComponentController component) {
        var width = component.placementRules.width;
        var height = component.placementRules.height;

        return new Vector3(Random.Range(0, width), 0, Random.Range(0, height));
    }
}
