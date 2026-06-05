using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static ShipComponentController;
using static UnityEngine.EventSystems.EventTrigger;

public class ShipController : MonoBehaviour
{
    public bool playerShip = true;
    public bool boss = false;
    public ShipBuildingController shipEditor;
    public Transform componentsParent;


    /// <summary>
    /// The original ship data.
    /// </summary>
    [SerializeField] public ShipData shipData;
    private bool everythingSolid;

    /// <summary>
    /// Ship data created during battle.
    /// For now unused, might be removed
    /// </summary>
    private ShipData battleShipData;

    [FormerlySerializedAs("componentGrid")]
    [SerializeField] private ComponentGrid _componentGrid;
    public ComponentGrid componentGrid { get => _componentGrid; private set => _componentGrid = value; }


    //ENERGY
    // Can take even without batteries
    [SerializeField] private int cabinEnergyCapacity = 1;
    public int storedEnergy = 0;
    public int batteryCapacity { get; private set; } = 0;
    private List<BatteryComponentController> batteries = new List<BatteryComponentController>();
    private MainCabinComponentController mainCabin;

    //CURRENCY PARTS
    public int storedMoney = 0;
    public SoundData moneyClip;
    public SoundData moneyBigClip;

    //Combat
    //Projectile spawnpoints
    public MissileSpawning missileSpawnPoints;

    // Whether generators should generate / etc.
    public bool componentsActive = false;
    public float DebugTextOffset = 0;

    public UnityEvent onEnergyChanged;


    private void Start()
    {
        // Create temporary ship data that will be used during game time
        //battleShipData = ScriptableObject.CreateInstance<ShipData>();
        //battleShipData = Instantiate(shipData);
        //componentGrid.ConnectGrid(battleShipData.componentGrid);

        AssignShipController();

        UpdateEnergyUI();
        mainCabin = componentGrid.GetComponentsOfType<MainCabinComponentController>()[0];
        if (playerShip && boss) Debug.LogError("Something went wrong,the player is the boss");
    }

    public void BuildShip()
    {
        Debug.Log("Build ship called");
        if (shipData == null) return;
        DeconstructShip();

        // NOTE: this way is stupid too, and breaks some things too

        // If not player => enemy
        // Rotate by 180 °, then set scale on all !!meshes!! to x = -1
        if (!playerShip)
        {
            // NOTE: rotating the components parent to avoid issues with targeting
            componentsParent.rotation = Quaternion.Euler(180, 180, 0);

            var oldPos = componentsParent.transform.position;
            componentsParent.transform.position = new Vector3(oldPos.x, 0, oldPos.z);
        }

        componentGrid = shipData.BuildShip(componentsParent);

        // NOTE: meshes after the grid !!!
        if (!playerShip)
        {
            /*foreach (var mesh in componentsParent.GetComponentsInChildren<MeshRenderer>())
            {
                var oldScale = mesh.gameObject.transform.localScale;
                mesh.gameObject.transform.localScale = new Vector3(oldScale.x, oldScale.y, -oldScale.z);

                var meshPos = mesh.gameObject.transform.localPosition;
                mesh.gameObject.transform.localPosition = new Vector3(meshPos.x, 1 - meshPos.y, meshPos.z);
            }*/

            // If we use all children, it affects the indicators as well
            foreach (Transform comp in componentsParent.transform)
            {
                foreach (Transform child in comp.transform)
                {
                    var mesh = child.GetComponent<MeshRenderer>();
                    if (mesh == null) continue;

                    var oldScale = mesh.gameObject.transform.localScale;
                    mesh.gameObject.transform.localScale = new Vector3(oldScale.x, oldScale.y, -oldScale.z);

                    var meshPos = mesh.gameObject.transform.localPosition;
                    mesh.gameObject.transform.localPosition = new Vector3(meshPos.x, 1 - meshPos.y, meshPos.z);
                
                }

                // Get the child named "Decor"
                var decor = comp.Find("Decor");
                if (decor != null)
                {
                    decor.localPosition = new Vector3(0.5f, 0.5f, 0.5f); 
                    // negate y pos of all children
                    foreach (Transform child in decor.transform)
                    {
                        child.localPosition = child.localPosition.SetY(-child.localPosition.y);
                    }
                }

            }

            var oldPos = componentsParent.transform.position;
            componentsParent.transform.position = new Vector3(oldPos.x, 1, oldPos.z);
        }

        Start();

        // Needs to be here for Unity to save the ship
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void DeconstructShip()
    {
        if (componentGrid == null) return;
        componentGrid.DestroyGrid();

        // This is just to be safe, because the above does not work after Unity restart for some reason...
        componentsParent.DestroyAllChildren();
    }

    /// <summary>
    /// Gives all components reference to this
    /// </summary>
    public void AssignShipController()
    {
        var components = componentGrid.GetAllComponents();
        foreach (var component in components)
        {
            component.SetShipController(this);
        }
    }

    /// <summary>
    /// Captures the current runtime state of the ship and returns it as a serializable ShipState
    /// Stores energy, money, health, and component states
    /// </summary>
    public ShipState SaveState()
    {
        ShipState state = new ShipState();

        if (shipData != null)
            state.shipDataName = shipData.name;

        state.storedEnergy = storedEnergy;
        state.storedMoney = storedMoney;

        var mainCabin = GetMainCabin();
        if (mainCabin != null)
        {
            state.mainCabinHealth = mainCabin.health;
            state.mainCabinMaxHealth = mainCabin.maxHealth;
        }

        if (componentGrid != null)
        {
            var allComponents = componentGrid.GetAllComponents();
            foreach (var component in allComponents)
            {
                if (component == null) continue;

                var cs = new ComponentState
                {
                    componentType = (int)component.componentType,
                    health = component.health,
                    maxHealth = component.maxHealth,
                    broken = component.broken
                };

                state.componentStates.Add(cs);
            }
        }

        Debug.Log($"Ship state saved: Energy={state.storedEnergy}, Money={state.storedMoney}, Components={state.componentStates.Count}");
        return state;
    }

    /// <summary>
    /// Rebuilds the ship from a saved ShipState
    /// If ShipData differs from the saved state, attempts to load it from Resources
    /// Then rebuilds the ship and restores all runtime values (energy, money, health, components)
    /// </summary>
    public void BuildFromState(ShipState state)
    {
        if (state == null)
        {
            Debug.LogWarning("BuildFromState called with null state");
            return;
        }

        // Try to load ShipData if the saved name differs
        if (!string.IsNullOrEmpty(state.shipDataName) && (shipData == null || shipData.name != state.shipDataName))
        {
            ShipData loadedData = Resources.Load<ShipData>(state.shipDataName);
            if (loadedData != null)
            {
                shipData = loadedData;
                Debug.Log($"Loaded ShipData '{state.shipDataName}' from Resources");
            }
            else
            {
                Debug.LogWarning($"Could not find ShipData '{state.shipDataName}' in Resources. Using current ShipData.");
            }
        }

        // Rebuild the ship's visual grid
        if (shipData != null)
        {
            BuildShip();
        }

        // Restore runtime state (energy, money, health, component conditions)
        storedEnergy = state.storedEnergy;
        storedMoney = state.storedMoney;

        var mainCabin = GetMainCabin();
        if (mainCabin != null)
        {
            mainCabin.health = (int)state.mainCabinHealth;
            mainCabin.maxHealth = (int)state.mainCabinMaxHealth;
        }

        if (componentGrid != null && state.componentStates != null)
        {
            var allComponents = componentGrid.GetAllComponents();
            foreach (var component in allComponents)
            {
                if (component == null) continue;

                var match = state.componentStates.Find(cs => cs.componentType == (int)component.componentType);
                if (match != null)
                {
                    component.health = (int)match.health;
                    // Note: 'broken' is read-only, so we can only restore health
                    // To set broken state, use a different method if available
                }
            }
        }

        // Reassign controller references and update UI
        if (componentGrid != null)
        {
            AssignShipController();
        }

        UpdateEnergyUI();

        Debug.Log($"Ship rebuilt from state: Energy={state.storedEnergy}, Money={state.storedMoney}");
    }




    /// <summary>
    /// Tells builder to start building with given components.
    /// </summary>
    /// <param name="componentsForPlacement">Components to place.</param>
    public void GiveControlToEditor(List<ShipComponentController> componentsForPlacement)
    {
        shipEditor.InitializeBuilder(componentGrid, componentsForPlacement);
    }

    /// <summary>
    /// Debug only. Starts placing with 3 components that are on the player.
    /// </summary>
    public void GiveControlToEditorDebug()
    {
        var componentsForPlacement = new List<ShipComponentController>();
        var components = componentGrid.GetAllComponents();

        for (int i = 0; i < 3; i++)
        {
            componentsForPlacement.Add(components[Random.Range(0, components.Count)]);
        }

        shipEditor.InitializeBuilder(componentGrid, componentsForPlacement);
    }

    public void RemoveControlFromEditor()
    {
        shipEditor.RemoveBuilderConnection();
    }




    public ShipComponentController GetMainCabin()
    {
        var maincabins = componentGrid.GetComponentsOfType<MainCabinComponentController>();
        if (maincabins.Count != 1)
        {
            Debug.LogError("Wrong number of main cabings " + maincabins.Count);
        }
        var maincabin = maincabins[0];

        return maincabin.GetComponent<ShipComponentController>();
    }


    public int GetEnergy => storedEnergy;

    /// <summary>
    /// Adds energy through the generator system, makes sure it fits the batteries
    /// </summary>
    /// <param name="originComponent">The component from which the energy originates. Add if you want particle effects</param>
    public void AddEnergy(int energy, ShipComponentController originComponent = null)
    {
        //TODO BETTER LOADING OF BATTERIES

        batteries = componentGrid.GetComponentsOfType<BatteryComponentController>(false);
        batteries.Shuffle();
        /* If living -> has cabin -> has at least some energy capacity
        if (batteries.Count == 0)
        {
            Debug.LogError("No batteries");
            return;
        }
        */
        batteryCapacity = (batteries.Count == 0 ? 0 : batteries.Count * batteries[0].energyMax) + cabinEnergyCapacity;

        // Debug.Log(storedEnergy +"+" + energy +", "+batteryCapacity);
        int totalEnergy = Mathf.Min(storedEnergy + energy, batteryCapacity);
        int remaining = totalEnergy - storedEnergy;

        // NOTE: maybe want to choose which battery takes the energy
        foreach (var component in batteries)
        {
            if (remaining == 0) break;
            int temp = remaining;
            remaining = component.Chargenergy(remaining);

            // Only send particles if some energy actually went into the baterry
            if (originComponent != null && temp != remaining)
                GameManager.Instance.SFXManager.EnergyTransmissionEffect(originComponent, component.shipComponentController);
        }

        if (remaining > 0)
            GameManager.Instance.SFXManager.EnergyTransmissionEffect(originComponent, mainCabin.shipComponentController);

        storedEnergy = totalEnergy;
        // Debug.Log("energy" + totalEnergy);
        onEnergyChanged.Invoke();

    }

    /// <summary>
    /// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
    /// </summary>
    /// <param name="originComponent">The component from which the energy originates. Add if you want particle effects</param>
    public bool UseEnergy(int energy, ShipComponentController originComponent = null)
    {
        if (batteries.Count == 0)//TODO BETTER LOADING OF BATTERIES
        {
            batteries = componentGrid.GetComponentsOfType<BatteryComponentController>();
            /*
            if(batteries.Count == 0) 
            {
                Debug.LogError("No batteries");
                return false;
            }
            */
            batteryCapacity = (batteries.Count == 0 ? 0 : batteries.Count * batteries[0].energyMax) + cabinEnergyCapacity;
        }
        batteries.Shuffle();
        //not enough energy
        if (storedEnergy - energy < 0)
        {
            return false;
        }

        int totalEnergy = Mathf.Max(storedEnergy - energy, 0);
        int remaining = storedEnergy - totalEnergy;
        foreach (var component in batteries)
        {
            if (remaining == 0) break;
            remaining = component.DrainEnergy(remaining);

            if (originComponent != null)
                GameManager.Instance.SFXManager.EnergyTransmissionEffect(component.shipComponentController, originComponent);
        }

        if (remaining > 0)
            GameManager.Instance.SFXManager.EnergyTransmissionEffect(mainCabin.shipComponentController, originComponent);

        storedEnergy = totalEnergy;
        onEnergyChanged.Invoke();
        return true;
    }

    public void UpdateEnergyUI()
    {
        batteryCapacity = (batteries.Count == 0 ? 0 : batteries.Count * batteries[0].energyMax) + cabinEnergyCapacity;
        onEnergyChanged.Invoke();
        // Debug.Log("Invoked energy change");
    }

    public int GetCurrency => storedMoney;
    public void AddCurrency(int amount)
    {
        // TODO: maybe limit via some components like energy

        storedMoney += amount;
        if (amount >= 5)
        {
            AudioManager.Instance.PlaySFX(moneyBigClip, transform.position);
        }
        else
        {
            AudioManager.Instance.PlaySFX(moneyClip, transform.position);
        }
    }

    public bool UseCurrency(int amount)
    {
        // not enough
        if (storedMoney - amount < 0)
            return false;

        // use it now
        storedMoney -= amount;
        AudioManager.Instance.PlaySFX(moneyClip, transform.position);
        return true;
    }


    // Remove stored energy, reset generators, batteries etc.
    // cooldowns 
    public void ResetComponentEffects()
    {
        // TODO: the rest...
        storedEnergy = 0;

        // NOTE: maybe okay to just call ResetBehaviour on all components ?
        var generators = componentGrid.GetComponentsOfType<GeneratorComponentController>();
        foreach (var gen in generators)
        {
            gen.ResetBehaviour();
        }


    }


    public void RepaireShip()
    {
        foreach (var component in componentGrid.GetAllComponents())
        {
            component.RepaireComponent();
        }
    }

    public void ResetShipForCombat()
    {
        if(!GameManager.Instance.currentGameplayManager.tutorialRunning)RepaireShip();
        storedEnergy = 0;
        var componentBehaviours = componentGrid.GetComponentsOfType<BehaviourComponentControllerAbstract>();
        foreach (var com in componentBehaviours)
        {
            com.ResetBehaviour();
        }
    }

    public void DisableShip()
    {
        /*
        foreach (var comp in componentGrid.GetAllComponents())
        {
            comp.enabled = false;
        }

        var behaviours = componentGrid.GetComponentsOfType<BehaviourComponentControllerAbstract>();
        foreach (var b in behaviours)
        {
            b.enabled = false;
        }
        */
        // 4. Schovej vizuál
        componentsParent.gameObject.SetActive(false);
    }

    public void EnableShip()
    {
        componentsParent.gameObject.SetActive(true);
        /*
        componentsParent.gameObject.SetActive(true);

        // 2. Zapni komponenty
        foreach (var comp in componentGrid.GetAllComponents())
        {
            comp.enabled = true;
        }

        // 3. Zapni behaviour skripty
        var behaviours = componentGrid.GetComponentsOfType<BehaviourComponentControllerAbstract>();
        foreach (var b in behaviours)
        {
            b.enabled = true;
        }
        */
    }
    /*
    void OnGUI()
    {
        if(playerShip)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.green;
            style.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(10 + DebugTextOffset, 10, 300, 40), $" {storedEnergy} / {cabinEnergyCapacity + batteryCapacity}", style);

            style.normal.textColor = Color.gray;
            GUI.Label(new Rect(10 + DebugTextOffset, 34, 300, 40), $" {storedMoney} $", style);
        }

    }*/

    public void DestroyEnemyComponents(List<ComponentType> componentTypes)
    {
        foreach (var componentType in componentTypes)
        {
            DestroyComponentsOfType(componentType);
        }
    }

    private void DestroyComponentsOfType(ComponentType type)
    {
        List<ShipComponentController> components = type switch
        {
            ComponentType.Generator =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<GeneratorComponentController>()),

            ComponentType.Battery =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<BatteryComponentController>()),

            ComponentType.Shield =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<ShieldComponentController>()),

            ComponentType.Rocket =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<MissileComponentController>()),

            ComponentType.Repaire =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<RepairerComponentController>()),

            ComponentType.Engine =>
                Utils.ConvertBehaviourListToComponentList(
                    componentGrid.GetComponentsOfType<EngineComponentController>()),

            _ => new List<ShipComponentController>()
        };

        foreach (var component in components)
        {
            if (component == null || component.broken)
                continue;

            component.KillComponent();
            return;
        }
    }



    /// <summary>
    /// disables colliders
    /// </summary>
    /// <param name="exceptionComponent">.</param>
    public void DisableAllCollidersExcept(ComponentType type = ComponentType.None, ComponentType type2 = ComponentType.None)
    {
        if (componentGrid == null) return;

        var allComponents = componentGrid.GetAllComponents();

        foreach (var comp in allComponents)
        {
            if (comp.componentType == type || comp.componentType == type2)
                continue;

            var colliders = comp.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }
    }

    /// <summary>
    /// turn them all on
    /// </summary>
    public void EnableAllColliders()
    {
        if (componentGrid == null) return;

        var allComponents = componentGrid.GetAllComponents();

        foreach (var comp in allComponents)
        {
            var colliders = comp.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }
        }
    }
}


