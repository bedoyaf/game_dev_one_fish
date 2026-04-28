using UnityEngine;

public class ShipComponentMeshController : MonoBehaviour, IDamagableCollider
{
    private ShipComponentController shipComponentController;

    private Collider meshCollider;

    private Renderer meshRenderer;
    private Color originalColor;
    [SerializeField] private Color brokenColor = Color.gray;

    private int deadLayer = -1;
    private int defaultLayer = -1;
   
    public void Awake()
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
        meshRenderer = GetComponent<Renderer>();
        originalColor  = meshRenderer.material.color;
    }
    public void OnMouseClick()
    {
        //CURRENTLY NOT SUPORTED TO CLICK ENEMY SHIP
        if (!shipComponentController.shipController.playerShip) return;
        Debug.Log("Component has been clicked, currently active:"+ shipComponentController.activated.ToString());
        if(!shipComponentController.activated)
        {
            shipComponentController.ActivateComponent();
        }
        else if(shipComponentController.activated)
        {
            shipComponentController.DeactivateComponent();
        }
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
            meshRenderer.material.color = brokenColor;
        }
    }

    public void ChangeMeshToWorking()
    {
        // meshCollider.enabled = true;
        gameObject.layer = defaultLayer;

        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }

    public void OnDamagableCollision(int amount)
    {
        shipComponentController.TakeDamage(amount);
    }
}
