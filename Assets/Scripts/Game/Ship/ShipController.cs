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
    public int storedEnergy = 0;
    private int batteryCapacity = 0;
    private List<BatteryComponentController> batteries = new List<BatteryComponentController>();


    //Combat
    //Projectile spawnpoints
    public Transform LeftProjectileSpawn;
    public Transform RightProjectileSpawn;
    public Transform UpProjectileSpawn;
    public Transform DownProjectileSpawn;

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
        componentGrid = shipData.BuildShip(componentsParent);

        AssignShipController();

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

    public void GiveControlToEditor() {
        var components = componentGrid.GetAllComponents();

        List<ShipComponentController> prefabs = new();
        for (int i = 0; i < 3; i++) {
            prefabs.Add(components[Random.Range(0, components.Count)].componentPrefab);
        }

        shipEditor.InitializeBuilder(componentGrid, prefabs);
    }

    public void RemoveControlFromEditor() {
        shipEditor.RemoveBuilderConnection();
    }




    public ShipComponentController GetMainCabin()
    {
        var maincabins = componentGrid.GetComponentsOfType<MainCabinComponentController>();
        if(maincabins.Count != 1)
        {
            Debug.LogError("Wrong number of main cabings");
        }
        var maincabin = maincabins[0];

        return maincabin.GetComponent<ShipComponentController>();
    }


    /// <summary>
    /// Adds energy through the generator system, makes sure it fits the batteries
    /// </summary>
    public void AddEnergy(int energy)
    {
        //TODO BETTER LOADING OF BATTERIES
        
        batteries = componentGrid.GetComponentsOfType<BatteryComponentController>(false);
        if (batteries.Count == 0)
        {
            Debug.LogError("No batteries");
            return;
        }
        batteryCapacity = batteries.Count * batteries[0].energyMax;
        
        
        int totalEnergy = Mathf.Min(storedEnergy+energy,batteryCapacity);
        int remaining = totalEnergy-storedEnergy;
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
            if(batteries.Count == 0) 
            {
                Debug.LogError("No batteries");
                return false;
            }
            batteryCapacity = batteries.Count * batteries[0].energyMax;
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

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(10+DebugTextOffset, 10, 300, 40), $" {storedEnergy} / {batteryCapacity}", style);
    }
}


