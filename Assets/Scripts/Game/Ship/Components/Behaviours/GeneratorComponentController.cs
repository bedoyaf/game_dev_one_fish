using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
/// </summary>
public class GeneratorComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] private int energyStored;
    public int GetCurrentEnergy => energyStored;

    [SerializeField] private int energyMax;
    public int GetEnergyCapacity => energyMax; 

    [SerializeField] private float energyPerSecond = 0.7f;

    private TextMesh debugText;

    private float energyBuffer = 0;

    [SerializeField] private SoundData gatherPowerClip;

    [SerializeField] private ParticleSystem generatorParticles;
    [SerializeField] private Vector2 emissionRate;
    private List<BatteryComponentController> batteries;
    private MainCabinComponentController mainCabin;

    public override bool CanClickOnNow => 
        !shipComponentController.broken 
        && GetCurrentEnergy > 0;

    private void Start() {
        if (shipComponentController.ComponentMesh.transform.localScale.z < 0) {
            generatorParticles.transform.localEulerAngles = new Vector3(180, 0, 0);
        }
    }

    public void DeleteEnergy()
    {
        energyBuffer = 0;
        energyStored = 0;
    }

    public override bool OnActivate()
    {
        RetreivePower();
        shipComponentController.DeactivateComponent();

        // Always can click (when no power, no reason to, but hey...)
        return true;
    }

    public override void OnAgentActivate(TargetingData data)
    {
        RetreivePower();
        shipComponentController.DeactivateComponent();
    }

    public override void ResetBehaviour()
    {
        energyBuffer = 0;
        energyStored = 0;
    }

    /// <summary>
    /// Returns stored power
    /// </summary>
    private void RetreivePower()
    {
        if (energyStored <= 0)
            return;

        Debug.Log($"Transferred {energyStored} energy to ship");

        //// Had issues when it was in Start...
        //if (batteries == null) {
        //    batteries = shipComponentController.shipController.componentGrid.GetComponentsOfType<BatteryComponentController>(false);
        //    mainCabin = shipComponentController.shipController.componentGrid.GetComponentsOfType<MainCabinComponentController>()[0];
        //}

        //// Store energy state before we add it
        //List<int> batteryEnergy = new();
        //foreach (var battery in batteries) {
        //    batteryEnergy.Add(battery.energyStored);
        //}
        //int shipEnergy = shipComponentController.shipController.GetEnergy;

        shipComponentController.shipController.AddEnergy(energyStored, shipComponentController);

        //int shipEnergyDiff = shipComponentController.shipController.GetEnergy - shipEnergy;

        energyStored = 0;

        var emissionModule = generatorParticles.emission;
        emissionModule.rateOverTime = emissionRate.x;

        AudioManager.Instance.PlaySFX(gatherPowerClip, transform.position);

        if(shipComponentController.shipController.playerShip)
        {
            // Spawn SFX
            GameManager.Instance.SFXManager.EnergyGatheredEffect(gameObject.transform.position);
        }

        //// ----------------------
        //// Spawn energy particles
        //int i = 0;
        //foreach (var battery in batteries) {
        //    int diff = battery.energyStored - batteryEnergy[i];
        //    if (diff > 0) {
        //        GameManager.Instance.SFXManager.EnergyTransmissionEffect(shipComponentController, battery.shipComponentController);
        //        shipEnergyDiff -= diff;
        //    }
        //}

        //if (shipEnergyDiff > 0) {
        //    GameManager.Instance.SFXManager.EnergyTransmissionEffect(shipComponentController, mainCabin.shipComponentController);
        //}

    }

    private void Update()
    {
        if(!shipComponentController.IsBroken && shipController.componentsActive) GenerateEnergy();
        // UpdateDebugText();//DEBUG
    }

    /// <summary>
    /// increments energy over time
    /// </summary>
    private void GenerateEnergy()
    {
        if (energyStored >= energyMax)
            return;

        energyBuffer += energyPerSecond * MyTime.deltaTime;

        while (energyBuffer >= 1f)
        {
            energyBuffer -= 1f;

            energyStored++;
            energyStored = Mathf.Min(energyStored, energyMax);

            var emissionModule = generatorParticles.emission;
            emissionModule.rateOverTime = Mathf.Lerp(emissionRate.x, emissionRate.y, (float)energyStored / energyMax);
        }
    }

    public override bool OnDeactivate() => true;

    public override bool OnTargetSelected(TargetingData target) => true;

    //DEBUG ----------------------------------------------------------------------------------------------
    /*
    void Start()
    {

        CreateDebugText();
    }
    */
    private void CreateDebugText()
    {
        GameObject textObj = new GameObject("EnergyDebugText");
        textObj.transform.SetParent(transform);

        debugText = textObj.AddComponent<TextMesh>();
        debugText.fontSize = 32;
        debugText.characterSize = 0.1f;
        debugText.anchor = TextAnchor.MiddleCenter;
        debugText.alignment = TextAlignment.Center;
        debugText.color = Color.black;

        var col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            textObj.transform.position = col.bounds.center;
        }
        else
        {
            textObj.transform.position = transform.position;
        }
    }
    private void UpdateDebugText()
    {
        if (debugText == null) return;

        debugText.text = $"{energyStored}/{energyMax}";

        debugText.transform.rotation = Camera.main.transform.rotation;
    }
}
