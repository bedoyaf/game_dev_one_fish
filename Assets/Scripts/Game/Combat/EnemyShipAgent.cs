using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyShipAgent : MonoBehaviour
{

    // TODO: set this to true, by default
    public bool thinking = false; 
    private float nextActTime;

    [SerializeField] private List<AgentBehavior> cycleBehaviors;
    private int currentBehaviorIndex = 0;
    private AgentBehavior CurrentBehavior => cycleBehaviors.Count > 0 ? 
        cycleBehaviors[currentBehaviorIndex] : AgentBehavior.EnergyCollection;

    // How often switch between behaviors
    [SerializeField] private float behaviorSwitchInterval = 2.5f;
    private float nextBehaviorSwitchTime;

    // How long does it take to complete one action (eg. to click a generator)
    [SerializeField] private float actActionDuration = 0.01f;

    [SerializeField] private float perfectShieldInterval = 5f; 
    private float perfectShieldTime = 0f;
    private bool perfectShieldReaction = true;

    private ShipController shipController;
    //player
    [SerializeField] private ShipController playerShip;
    public void SetPlayerShip(ShipController player)
    {
        playerShip = player;
    }

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

        // Subscribe to incoming projectile notifications on player components
        SubscribeToPlayerIncomingProjectiles();

    }

    private void SubscribeToPlayerIncomingProjectiles()
    {
        Debug.Log("Subscribing to player incoming projectile events...");
        if (shipController == null) return;

        var playerComponents = shipController.componentGrid.GetAllNonBrokenComponents();

        foreach (var comp in playerComponents)
        {
            // Ensure event exists
            if (comp.OnIncomingProjectile == null)
                comp.OnIncomingProjectile = new UnityEngine.Events.UnityEvent<ShipComponentController>();

            // Remove previous listeners to avoid duplicates (best-effort)
            try {
                // PASS THE METHOD GROUP (delegate) instead of calling the method.
                comp.OnIncomingProjectile.RemoveListener(OnIncomingProjectileForComponent);
            } catch { }

            // Add listener that will receive the ShipComponentController argument from the event
            comp.OnIncomingProjectile.AddListener(OnIncomingProjectileForComponent);
        }
    }

    private void OnIncomingProjectileForComponent(ShipComponentController targetComponent)
    {
        Debug.Log("Incoming missile, raise shield on "+ targetComponent.name);
        if(!perfectShieldReaction) return;
        // Decide whether to spawn a shield in reaction. Simple strategy: use first available shield component.
        foreach (var shieldComp in shields)
        {
            if (shieldComp == null) continue;
            if (shieldComp.broken) continue;
            if (shieldComp.activated) continue;

            // Activate this shield targeting the incoming component
            shieldComp.AgentActivateComponent(new TargetingData(targetComponent.shipComponentMeshController));
            perfectShieldReaction = false; 
            return;
        }
    }


    public void ComponentRemoved()
    {
        // update lists (parts of the ship might have been completely destroyed, so best
        // to just update all the lists)

        SetupAllImportantSystems();
    }



    //Missleading name, hope in future it fits better
    public void ActivateAgent()
    {
        SetupAllImportantSystems();
    }

    // Targeting modes for choosing which player component to attack
    enum TargetingMode
    {
        CabinRaycast, // Target the cabin, choose approach direction by raycast
        Weapon,       // Specifically target player's weapon (missile) components
        Generator,    // Specifically target player's generators
        Structural,   // Target structural/solid components
        Engine,       // Target engine components
        Random        // Random non-broken component
    }

    [SerializeField] private TargetingMode targetingMode = TargetingMode.Random;
    
    [Header("Targeting Cycle")]
    [SerializeField] private bool cycleTargeting = true;
    [SerializeField] private float targetingSwitchInterval = 6.0f;
    private float nextTargetingSwitchTime;

    // Direction chosen for current target (used for cabin targeting)
    private Vector3? lastChosenDirection = null;


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
            nextActTime = MyTime.time + actActionDuration;
        }

        // Cycle targeting modes periodically for variety
        if (cycleTargeting && MyTime.time >= nextTargetingSwitchTime)
        {
            CycleTargetingMode();
            nextTargetingSwitchTime = MyTime.time + targetingSwitchInterval;
        }

        if (MyTime.time >= perfectShieldTime && thinking)
        {
            perfectShieldReaction = true;
            perfectShieldTime = MyTime.time + perfectShieldInterval;
        }
    }

    private void CycleTargetingMode()
    {
        var values = System.Enum.GetValues(typeof(TargetingMode));
        int idx = System.Array.IndexOf(values, targetingMode);
        idx = (idx + 1) % values.Length;
        targetingMode = (TargetingMode)values.GetValue(idx);
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

    void ActivateGenerator()
    {
        List<ShipComponentController> shuffledGenerators = new List<ShipComponentController>(generators);

        for (int i = 0; i < shuffledGenerators.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledGenerators.Count);

            (shuffledGenerators[i], shuffledGenerators[randomIndex]) =
                (shuffledGenerators[randomIndex], shuffledGenerators[i]);
        }

        foreach (var gen in shuffledGenerators)
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

    void FireWeapon()
    {
        var target = GetTargetForWeapon();
        if (target == null) return;

        foreach (var weapon in missiles)
        {
            // BROKEN really important here!!!
            if (!weapon.broken && !weapon.activated)
            {


                // pick a direction. If we previously computed a direction for cabin targeting, use it.
                Vector3 dir;
                if (target == playerCabin && lastChosenDirection.HasValue)
                {
                    dir = lastChosenDirection.Value;
                    // clear it so next time we re-evaluate
                    lastChosenDirection = null;
                }
                else
                {
                    dir = MouseController.ENEMY_DIRECTIONS[Random.Range(0, 3) % 3];
                }

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

    // Choose a target based on the configured targeting mode. Returns a non-broken ShipComponentController or null.
    private ShipComponentController GetTargetForWeapon()
    {
        if (playerShip == null) return null;

        switch (targetingMode)
        {
            case TargetingMode.CabinRaycast:
                // Always target the player's cabin, but choose the direction that has the fewest
                // blocking player components between the chosen approach and the cabin.
                if (playerCabin == null) break;

                var cabinPos = playerCabin.shipComponentMeshController?.transform.position ?? playerCabin.transform.position;

                Vector3 bestDir = MouseController.ENEMY_DIRECTIONS[0];
                int bestCount = int.MaxValue;

                foreach (var d in MouseController.ENEMY_DIRECTIONS)
                {
                    // Start the ray well outside the ship in this direction and cast towards the cabin
                    Vector3 from = cabinPos + d.normalized * 20f;
                    Vector3 rayDir = (cabinPos - from).normalized;

                    var hitsDir = Physics.RaycastAll(new Ray(from, rayDir), 40f);

                    int count = 0;
                    foreach (var h in hitsDir.OrderBy(h => h.distance))
                    {
                        var mesh = h.collider.GetComponentInParent<ShipComponentMeshController>();
                        if (mesh == null) continue;
                        var comp = mesh.transform.parent.GetComponent<ShipComponentController>();
                        if (comp == null) continue;

                        // If we've reached the cabin, stop counting
                        if (comp == playerCabin) break;

                        if (comp.shipController == playerShip)
                            count++;
                    }

                    if (count < bestCount)
                    {
                        bestCount = count;
                        bestDir = d;
                    }
                }

                lastChosenDirection = bestDir;
                return playerCabin;

            case TargetingMode.Weapon:
                var weapons = playerMissiles.Where(m => m != null && !m.broken).ToList();
                if (weapons.Count > 0) return weapons[Random.Range(0, weapons.Count)];
                break;

            case TargetingMode.Generator:
                var gens = playerGenerators.Where(g => g != null && !g.broken).ToList();
                if (gens.Count > 0) return gens[Random.Range(0, gens.Count)];
                break;

            case TargetingMode.Structural:
                // Prefer components that are marked solid in placement rules
                var structural = playerShip.componentGrid.GetAllNonBrokenComponents()
                    .Where(c => c != null)
                    .ToList();
                if (structural.Count > 0) return structural[Random.Range(0, structural.Count)];
                break;

            case TargetingMode.Engine:
                // Try to select engine components first
                var engines = playerShip.componentGrid.GetAllNonBrokenComponents()
                    .Where(c => c != null && c.GetComponent<EngineComponentController>() != null)
                    .ToList();
                if (engines.Count > 0) return engines[Random.Range(0, engines.Count)];
                break;

            case TargetingMode.Random:
                var comps = playerShip.componentGrid.GetAllNonBrokenComponents();
                if (comps.Count > 0) return comps[Random.Range(0, comps.Count)];
                break;
        }

        // fallback to weakest component
        return GetWeakestComponent(playerShip);
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

    void OnGUI()
    {
        if(GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey == GameplayFlowManager.GameStates.Combat)
        {
            float width = 140;
            float height = 40;

            Rect rect = new Rect(10, Screen.height - height - 10, width, height);

            string label = thinking ? "EnemyAI: ON" : "EnemyAI: OFF";

            if (GUI.Button(rect, label))
            {
                thinking = !thinking;
            }
        }
    }
}
