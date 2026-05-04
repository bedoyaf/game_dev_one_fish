using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.EventSystems.EventTrigger;

public class ShipController : MonoBehaviour
{
    public bool playerShip = true;
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
    private int batteryCapacity = 0;
    private List<BatteryComponentController> batteries = new List<BatteryComponentController>();

    //CURRENCY PARTS
    public int storedMoney = 0;


    //Combat
    //Projectile spawnpoints
    public MissileSpawning missileSpawnPoints;

    public float DebugTextOffset = 0;


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
        if(!playerShip)
        {
            /*foreach (var mesh in componentsParent.GetComponentsInChildren<MeshRenderer>())
            {
                var oldScale = mesh.gameObject.transform.localScale;
                mesh.gameObject.transform.localScale = new Vector3(oldScale.x, oldScale.y, -oldScale.z);

                var meshPos = mesh.gameObject.transform.localPosition;
                mesh.gameObject.transform.localPosition = new Vector3(meshPos.x, 1 - meshPos.y, meshPos.z);
            }*/

            // If we use all children, it affects the indicators as well
            foreach (Transform comp in componentsParent.transform) {
                foreach (Transform child in comp.transform) {
                    var mesh = child.GetComponent<MeshRenderer>();
                    if (mesh == null) continue;

                    var oldScale = mesh.gameObject.transform.localScale;
                    mesh.gameObject.transform.localScale = new Vector3(oldScale.x, oldScale.y, -oldScale.z);

                    var meshPos = mesh.gameObject.transform.localPosition;
                    mesh.gameObject.transform.localPosition = new Vector3(meshPos.x, 1 - meshPos.y, meshPos.z);
                }
            }

            var oldPos = componentsParent.transform.position;
            componentsParent.transform.position = new Vector3(oldPos.x, 1, oldPos.z);
        }

        AssignShipController();

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
    public void AssignShipController() {
        var components = componentGrid.GetAllComponents();
        foreach (var component in components) {
            component.SetShipController(this);
        }
    }

    /// <summary>
    /// Tells builder to start building with given components.
    /// </summary>
    /// <param name="componentsForPlacement">Components to place.</param>
    public void GiveControlToEditor(List<ShipComponentController> componentsForPlacement) {
        shipEditor.InitializeBuilder(componentGrid, componentsForPlacement);
    }

    /// <summary>
    /// Debug only. Starts placing with 3 components that are on the player.
    /// </summary>
    public void GiveControlToEditorDebug() {
        var componentsForPlacement = new List<ShipComponentController>();
        var components = componentGrid.GetAllComponents();

        for (int i = 0; i < 3; i++) {
            componentsForPlacement.Add(components[Random.Range(0, components.Count)]);
        }

        shipEditor.InitializeBuilder(componentGrid, componentsForPlacement);
    }

    public void RemoveControlFromEditor() {
        shipEditor.RemoveBuilderConnection();
    }




    public ShipComponentController GetMainCabin()
    {
        var maincabins = componentGrid.GetComponentsOfType<MainCabinComponentController>();
        if(maincabins.Count != 1)
        {
            Debug.LogError("Wrong number of main cabings "+ maincabins.Count);
        }
        var maincabin = maincabins[0];

        return maincabin.GetComponent<ShipComponentController>();
    }


    public int GetEnergy => storedEnergy;

    /// <summary>
    /// Adds energy through the generator system, makes sure it fits the batteries
    /// </summary>
    public void AddEnergy(int energy)
    {
        //TODO BETTER LOADING OF BATTERIES
        
        batteries = componentGrid.GetComponentsOfType<BatteryComponentController>(false);
        /* If living -> has cabin -> has at least some energy capacity
        if (batteries.Count == 0)
        {
            Debug.LogError("No batteries");
            return;
        }
        */
        batteryCapacity = batteries.Count == 0 ? 0 : batteries.Count * batteries[0].energyMax;


        int totalEnergy = Mathf.Min(storedEnergy+energy, cabinEnergyCapacity + batteryCapacity);
        int remaining = totalEnergy-storedEnergy;

        // NOTE: maybe want to choose which battery takes the energy
        foreach (var component in batteries)
        {
            if (remaining == 0) break;
            remaining = component.Chargenergy(remaining);
        }
        storedEnergy = totalEnergy;
        Debug.Log("energy" + totalEnergy);
    }

    /// <summary>
    /// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
    /// </summary>
    public bool UseEnergy(int energy)
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
            batteryCapacity = batteries.Count == 0 ? 0 : batteries.Count * batteries[0].energyMax;
        }
        //not enough energy
        if(storedEnergy-energy<0)
        {
            return false;
        }

        int totalEnergy = Mathf.Max(storedEnergy - energy, 0);
        int remaining = storedEnergy-totalEnergy;
        foreach (var component in batteries)
        {
            if (remaining == 0) break;
            remaining = component.DrainEnergy(remaining);
        }
        storedEnergy = totalEnergy;

        return true;
    }

    public int GetCurrency => storedMoney;
    public void AddCurrency(int amount)
    {
        // TODO: maybe limit via some components like energy

        storedMoney += amount;
    }

    public bool UseCurrency(int amount)
    {
        // not enough
        if (storedMoney - amount < 0)
            return false;

        // use it now
        storedMoney -= amount;
        return true;
    }


    public void RepaireShip()
    {
        foreach(var component in componentGrid.GetAllComponents())
        {
            component.RepaireComponent();
        }
    }

    public void ResetShipForCombat()
    {
        RepaireShip();
        storedEnergy = 0;
        var componentBehaviours = componentGrid.GetComponentsOfType<BehaviourComponentControllerAbstract>();
        foreach(var com in componentBehaviours)
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

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(10 + DebugTextOffset, 10, 300, 40), $" {storedEnergy} / {cabinEnergyCapacity+batteryCapacity}", style);

        style.normal.textColor = Color.gray;
        GUI.Label(new Rect(10 + DebugTextOffset, 34, 300, 40), $" {storedMoney} $", style);

    }
}


