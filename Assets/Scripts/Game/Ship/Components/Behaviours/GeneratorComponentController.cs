using UnityEngine;

/// <summary>
/// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
/// </summary>
[RequireComponent(typeof(ShipComponentController))]
public class GeneratorComponentController : BehaviourComponentControllerAbstract
{
    [SerializeField] private int energyStored;
    [SerializeField] private int energyMax;

    [SerializeField] private float energyPerSecond = 0.7f;

    private TextMesh debugText;

    private float energyBuffer =0;

    public override void OnActivate()
    {
        RetreivePower();
        shipComponentController.DeactivateComponent();
    }

    /// <summary>
    /// Returns stored power
    /// </summary>
    private void RetreivePower()
    {
        if (energyStored <= 0)
            return;

        Debug.Log($"Transferred {energyStored} energy to ship");

        shipComponentController.shipController.AddEnergy(energyStored);

        energyStored = 0;
    }

    private void Update()
    {
        GenerateEnergy();
        UpdateDebugText();//DEBUG
    }

    /// <summary>
    /// increments energy over time
    /// </summary>
    private void GenerateEnergy()
    {
        if (energyStored >= energyMax)
            return;

        energyBuffer += energyPerSecond * Time.deltaTime;

        while (energyBuffer >= 1f)
        {
            energyBuffer -= 1f;

            energyStored++;
            energyStored = Mathf.Min(energyStored, energyMax);
        }
    }

    public override void OnDeactivate(){}

    public override void OnTargetSelected(TargetingData target){}

    //DEBUG ----------------------------------------------------------------------------------------------
    void Start()
    {
        CreateDebugText();
    }

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
