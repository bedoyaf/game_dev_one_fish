using Unity.VisualScripting;
using UnityEngine;

public class ShipComponentMeshController : MonoBehaviour, IDamagableCollider
{
    private ShipComponentController shipComponentController;

    private Collider meshCollider;

    [SerializeField] private Renderer meshRenderer;
    private Color originalColor;
    [SerializeField] private Color brokenColor = Color.gray;
    // some meshes have multiple materials -> pick which one
    [SerializeField] private int materialIndex = 0;

    private int deadLayer = -1;
    private int defaultLayer = -1;

    // Whether this component belongs to the player ship
    public bool BelongsToPlayer => shipComponentController.shipController.playerShip;


    public void Start()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        deadLayer = LayerMask.NameToLayer("DeadComponent");

        if (shipComponentController == null)
        {
            shipComponentController = GetComponentInParent<ShipComponentController>();
            shipComponentController.shipComponentMeshController = this;
        }
        if (shipComponentController == null) Debug.LogError("ShipComponentMeshController lacks the parent script");
        meshCollider = GetComponent<Collider>();
        //meshRenderer = GetComponent<Renderer>();

        originalColor  = meshRenderer.materials[materialIndex].color;
        meshRenderer.material.SetFloat("damageAmount", 0);
    }
    public bool OnMouseClick()
    {
        //CURRENTLY NOT SUPORTED TO CLICK ENEMY SHIP
        if (!shipComponentController.shipController.playerShip) return false;
       // Debug.Log("Component has been clicked, currently active:"+ shipComponentController.activated.ToString());
        if(!shipComponentController.activated)
        {
            return shipComponentController.ActivateComponent();
        }
        else if(shipComponentController.activated)
        {
            return shipComponentController.DeactivateComponent();
        }

        return false;
    }

    public void OnRepairClick()
    {
        if (!shipComponentController.shipController.playerShip) return;
        Debug.Log("Component has been clicked, to repair:" + shipComponentController.health.ToString());

        shipComponentController.RepairClick();
    }

    


    public void ChangeMeshToBroken()
    {
        // meshCollider.enabled = false;
        gameObject.layer = deadLayer;
        
        if (meshRenderer != null)
        {
            meshRenderer.materials[materialIndex].color = brokenColor;
        }
    }

    public void ChangeMeshToWorking()
    {
        // meshCollider.enabled = true;
        gameObject.layer = defaultLayer;

        if (meshRenderer != null)
        {
            meshRenderer.materials[materialIndex].color = originalColor;
        }
    }

    // Call to set damage based on fraction of total Heatlh
    public void OnHealthUpdate(float fraction)
    {
        if (meshRenderer != null)
        {
            //Debug.Log($"Changing damage of material to {1f - fraction}"); // Sorry thiw was just spamming too much
            // NOTE: sometimes a really large value gets to the thingy and the effect is strange
            // not sure why yet...
            // HOTFIX
            var value = Mathf.Max(0.0f, Mathf.Min(1.0f, 1f - fraction));
            meshRenderer.materials[materialIndex].SetFloat("_damageAmount", value);
        }
    }
    public void OnDamagableCollision(int amount)
    {
        shipComponentController.TakeDamage(amount);
    }
}
