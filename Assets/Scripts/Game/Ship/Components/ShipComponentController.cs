using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// main components manages, handles activation, damage and more
/// </summary>
public class ShipComponentController : MonoBehaviour
{
    [SerializeField] public string componentName = "No name";
    [SerializeField] public ComponentType componentType = ComponentType.None;
    public int maxHealth = 10;
    public int health = 10;
    public bool IsBroken => broken;

    private int healthPerRepair = 5;

    public UnityEvent<ShipComponentController> OnDeath;
    // Event fired when a projectile is predicted to hit this component.
    public UnityEvent<ShipComponentController> OnIncomingProjectile;
    public bool activated = false;

    public int requiredEnergy = 0;

    // how much currency added if destroyed by the player
    public int destroyRevenue = 1;

    private IShipComponentBehaviour componentBehaviour;

    public ShipController shipController { get; private set; }

    public GameObject ComponentMesh; // The child of the component, that has the mesh on it
    public GameObject ComponentHitbox; // The child of the component that has the hitbox on it
    
    private GameObject outlineMesh;
    public Shield shield { private set; get; }

    [HideInInspector] public ShipComponentMeshController shipComponentMeshController;
    private ComponentCooldown cooldown;

    public bool broken {  get; private set; } = false;

    public ComponentPrefabsData componentPrefabs;
    public string guid;

    [Header("Sounds")]
    [FormerlySerializedAs("ActivationClip")]
    public AudioClip activationClip;
    public AudioClip repairClip;
    public AudioClip breakClip;

    [Header("Builder stuff")]

    //public ShipComponentController componentPrefab; // Very useful for the builder!
    public ShipComponentController ComponentPrefab => componentPrefabs.GetPrefab(guid);

    public ComponentPlacement placementRules;

    //  public UnityEvent OnBroken; Ondeath preferable 

    void Start()
    {
        componentBehaviour = GetComponent<IShipComponentBehaviour>();
      //  componentBehaviour.set
        if (componentBehaviour == null) Debug.LogWarning("Missing behaviour");//Make it error
        cooldown = GetComponent<ComponentCooldown>();
    }

    /// <summary>
    /// Called by mesh controller when a projectile is predicted to hit this component.
    /// Forwards the notification via an event so other systems can react (shields, UI, etc).
    /// </summary>
    public void IncomingProjectile(Vector3 impactPoint, float timeToImpact, Projectile projectile)
    {
        // Default behavior: if shield exists, let shield know (shield may want to activate or block)
        Debug.Log($"Incoming projectile in {timeToImpact}s at {impactPoint} to {name}");

        OnIncomingProjectile?.Invoke(this);
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetShipController(ShipController shipController)
    {
        this.shipController = shipController;
        var behaviors = GetComponentsInChildren<BehaviourComponentControllerAbstract>();
        foreach (var b in behaviors) {
            b.SetShipController(shipController);
        }
    }

    public void TakeDamage(int dmg)
    {

        if (shield != null)
        {
            Debug.Log("Shield catched damage");
            shield.TakeDamage(dmg);
            return;
        }

    //    Debug.Log("No shield -> damaging HP");

        health -= dmg;
        shipComponentMeshController.OnHealthUpdate((float)health / maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    public bool ActivateComponent()
    {
        if(broken) return false;
        if (componentBehaviour == null) return false;

        if (cooldown != null && !cooldown.IsReady)
        {
            Debug.Log("Component is on cooldown");
            return false;
        }

        if (!activated)
        {
            if(!shipController.UseEnergy(requiredEnergy))
            {
                Debug.Log("Not enaugh energy");
                return false;
            }

            AudioManager.Instance.PlaySFX(activationClip, transform.position);
            activated = true;
            return componentBehaviour.OnActivate();
        }

        return false;
    }

    public void AgentActivateComponent(TargetingData target = null)
    {
        if (broken) return;
        if (componentBehaviour == null) return;

        if (cooldown != null && !cooldown.IsReady)
        {
            return;
        }

        if (!activated)
        {
            if (!shipController.UseEnergy(requiredEnergy))
            {
                return;
            }

            activated = true;
            componentBehaviour.OnAgentActivate(target);
        }
    }

    public bool DeactivateComponent()
    {
        if(activated)
        {
            var res = componentBehaviour.OnDeactivate();
            activated = false;

            return res;
        }

        return false;
    }

    private void Die()//TODO MAKE BROKEN VERSION OF COMPONENT
    {
        OnDeath?.Invoke(this);

        // Award money 
        CombatController.Instance.ComponentDestroyed(this, shipController);

        // Destroys the component and works with the grid.
        //shipController.componentGrid.OnComponentDeath(placementRules.connectedTile);
        BreakComponent();


    }

    public void BreakOff() {
        // Set the bool at the end to true if this component should be included to separated components
        var separatedComponents = shipController.componentGrid.GetAllSeparatedComponentsAfterRemoval(placementRules.connectedTile, false);
        
        // Recursively removes all connected components from the grid memory (will not destroy the objects)
        shipController.componentGrid.RemoveComponent(placementRules.connectedTile, true, false);

        // Do whatever you want with the components
        StartCoroutine(MoveDown(separatedComponents));
    }
    // This is just to showcase the code, feel free to remove
    private IEnumerator MoveDown(List<ShipComponentController> wouldDissappear) {
        for (int i = 0; i < 1000; i++) {
            yield return null;
            foreach (var comp in wouldDissappear) {
                comp.transform.position -= new Vector3(0, 0, 1.0f / 60);
            }
        }
    }


    private void BreakComponent()
    {
        broken = true;
        shipComponentMeshController.ChangeMeshToBroken();
        shipComponentMeshController.OnHealthUpdate(0f);
        AudioManager.Instance.PlaySFX(breakClip, transform.position);
        shipController.UpdateEnergyUI();
    }


    private float baseOutlineWidth = 1.0f;
    private float fadeTime;
    private Color currentHightlightColor = Color.black;
    public void Highlight(Material highlightMaterial, Color color, float outlineSize, float fadeTime) {
        this.fadeTime = fadeTime;
        currentHightlightColor = color;
        MeshRenderer mesh;
        if (outlineMesh == null) {
            outlineMesh = new GameObject($"{name} highlight");
            outlineMesh.transform.parent = transform;
            outlineMesh.transform.position = ComponentMesh.transform.position;
            outlineMesh.transform.rotation = ComponentMesh.transform.rotation;
            outlineMesh.transform.localScale = ComponentMesh.transform.localScale;

            var filter = outlineMesh.AddComponent<MeshFilter>();
            filter.mesh = ComponentMesh.GetComponent<MeshFilter>().mesh;
            mesh = outlineMesh.AddComponent<MeshRenderer>();

        }
        else {
            outlineMesh.SetActive(true);
            mesh = outlineMesh.GetComponent<MeshRenderer>();
        }

        int materialsCount = ComponentMesh.GetComponent<MeshRenderer>().materials.Length;
        var materials = new List<Material>();
        for (int i = 0; i < materialsCount; i++) {
            materials.Add(new Material(highlightMaterial));
            materials[i].SetColor("_Color", color);
            materials[i].SetFloat("_OutlineSize", baseOutlineWidth);
        }

        mesh.materials = materials.ToArray();
        StartCoroutine(FadeOutline(materials, outlineSize, fadeTime, false));
    }

    public void ChangeHighlightColor(Color color) {
        if (color == currentHightlightColor || outlineMesh == null) return;

        currentHightlightColor = color;
        var materials = outlineMesh.GetComponent<MeshRenderer>().materials;
        foreach (var mat in materials) {
            mat.DOColor(color, fadeTime);
        }
    }

    private IEnumerator FadeOutline(List<Material> highlightMaterials, float target, float fadeTime, bool disable) {
        foreach(var mat in highlightMaterials) {
            mat.DOFloat(target, "_OutlineSize", fadeTime);
        }

        if (disable) {
            yield return MyTime.WaitForSeconds(fadeTime);
            outlineMesh.SetActive(false);
        }
    }

    public void RemoveHighlight() {
        if (outlineMesh != null) {
            StartCoroutine(FadeOutline(outlineMesh.GetComponent<MeshRenderer>().materials.ToList(), baseOutlineWidth, fadeTime, true));
        }
    }


    public void RepairFromComponent()
    {

    }

    // NOTE: maybe could adjust ?
    private int repairCost = 1;

    public bool CanRepairThisComponent =>
        health < maxHealth &&
        shipController.GetCurrency > repairCost;

    public void RepairClick()
    {
        if(CanRepairThisComponent)
        {
            Debug.Log("Can repair");

            // use the currency
            shipController.UseCurrency(1);

            // do the repair
            health = Math.Min(health + healthPerRepair, maxHealth);
            shipComponentMeshController.OnHealthUpdate((float)health / maxHealth);
            AudioManager.Instance.PlaySFX(repairClip, transform.position);

            if (broken)
            {
                Debug.Log("Revive");
                broken = false;
                shipComponentMeshController.ChangeMeshToWorking();
            }

        } else
        {  
            Debug.Log("Can't repair, reasons...");
        }
    }


    public void RepaireComponent()
    {
        broken = false;
        health = maxHealth;
        //can be initialized in the wrong order
        if (shipComponentMeshController != null)
        {
            shipComponentMeshController.ChangeMeshToWorking();
            shipComponentMeshController.OnHealthUpdate((float)health / maxHealth);
        }
    }

    public void ActivateShield(Shield shield)
    {
        Debug.Log("Shield activated");
        this.shield = shield;
        shield.OnShieldDestroyed.AddListener(ResetShield);
    }

    private void ResetShield(Shield shield)
    {
        Debug.Log("Shield gone");
        if (shield != null) Destroy(shield);
        this.shield = null;
    }


    [Serializable]
    public class ComponentPlacement {
        public int width = 1;
        public int height = 1;

        public bool solid;

        [Header("Blocks to sides")]
        public int top;
        public int right;
        public int bottom;
        public int left;

        public bool blockSurroundings => top != 0 || right != 0 || bottom != 0 || left != 0;

        public ComponentGridTile connectedTile; // TODO - sorry, does not belong here
    }


    // NOTE: maybe move to separate script ?
    [SerializeField] private ComponentDescription myDescription;
    public ComponentDescription GetDescription() => myDescription;

    public override string ToString()
    {
        return componentName;
    }

    /// <summary>
    /// Used for identifying components from events.
    /// </summary>
    public enum ComponentType {
        None,
        Battery,
        Engine,
        Generator,
        MainCabin,
        Rocket,
        Shield,
        Repaire

    }

}

[Serializable]
public class ComponentDescription
{
    public bool ignore = true;

    public string displayName;
    public string costs;
    public string textDescription;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
}