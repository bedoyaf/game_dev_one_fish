using UnityEngine;

/// <summary>
/// Battery component, just stores values and has charge drain functions
/// </summary>
public class BatteryComponentController : BehaviourComponentControllerAbstract
{

    public int energyStored = 0;
    public int energyMax = 10;
    private TextMesh debugText;
    public override void OnActivate()
    {
        shipComponentController.DeactivateComponent();
    }

    public override void OnAgentActivate(TargetingData data)
    {
        shipComponentController.DeactivateComponent();
    }

    public override void OnDeactivate()
    {

    }

    public override void OnTargetSelected(TargetingData target)
    {

    }

    public int Chargenergy(int energy)
    {
        energyStored += energy;
        int remaining = energyStored- energyMax;
        energyStored = Mathf.Min(energyMax, energyStored);
        return Mathf.Max(0, remaining);
    }

    public int DrainEnergy(int energy)
    {
        energyStored -= energy;
        int remaining = -energyStored;
        return Mathf.Max(0, remaining);
    }

    /// Debug texts ----------------------------------------------------------------------------------------------------------------

    void Start()
    {
        CreateDebugText();
    }

    private void Update()
    {
        UpdateDebugText();
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
