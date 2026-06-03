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
        StopGeneratorParticles();
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
        StopGeneratorParticles();
    }

    /// <summary>
    /// Returns stored power
    /// </summary>
    private void RetreivePower()
    {
        if (energyStored <= 0)
            return;

        Debug.Log($"Transferred {energyStored} energy to ship");

        shipComponentController.shipController.AddEnergy(energyStored, shipComponentController);

        //int shipEnergyDiff = shipComponentController.shipController.GetEnergy - shipEnergy;

        energyStored = 0;
        StopGeneratorParticles();

        AudioManager.Instance.PlaySFX(gatherPowerClip, transform.position);

        if(shipComponentController.shipController.playerShip)
        {
            // Spawn SFX
            GameManager.Instance.SFXManager.EnergyGatheredEffect(gameObject.transform.position);
        }

        //// ----------------------
    }

    private void StopGeneratorParticles() {
        var emissionModule = generatorParticles.emission;
        emissionModule.rateOverTime = emissionRate.x;
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
