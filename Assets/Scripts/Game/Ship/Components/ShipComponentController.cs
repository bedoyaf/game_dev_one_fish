using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
/// </summary>
public class ShipComponentController : MonoBehaviour
{
    public int maxHealth = 10;
    public int health = 10;
    public UnityEvent<ShipComponentController> OnDeath;
    public bool activated = false;
    [SerializeField] private bool requiresPower = true;

    public int requiredEnergy = 0;

    private IShipComponentBehaviour componentBehaviour;

    [Header("Builder stuff")]
    public ComponentPlacement placementRules;

    public ShipController shipController { get; private set; }

    public ShipComponentController componentPrefab; //TODO, might be useless delete these
    public GameObject ComponentMesh; // The child of the component, that has the mesh on it
    
    private Shield shield;

    public ShipComponentMeshController shipComponentMeshController;

    public bool broken {  get; private set; } = false;

    void Start()
    {
        componentBehaviour = GetComponent<IShipComponentBehaviour>();
        if (componentBehaviour == null) Debug.LogWarning("Missing behaviour");//Make it error
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetShipController(ShipController shipController)
    {
        this.shipController = shipController;
    }

    public void TakeDamage(int dmg)
    {
        if(shield!=null)
        {
            shield.TakeDamage(dmg);
        }

        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ActivateComponent()
    {
        if(broken) return;

        if(!activated && componentBehaviour!=null)
        {
            if(!shipController.UseEnergy(requiredEnergy))
            {
                Debug.Log("Not enaugh energy");
                return;
            }

            activated = true;
            componentBehaviour.OnActivate();
        }
    }

    public void DeactivateComponent()
    {
        if(activated)
        {
            componentBehaviour.OnDeactivate();
            activated = false;
        }
    }

    private void Die()//TODO MAKE BROKEN VERSION OF COMPONENT
    {
        OnDeath?.Invoke(this);

        // Destroys the component and works with the grid.
        //shipController.componentGrid.OnComponentDeath(placementRules.connectedTile);
        BreakComponent();
    }

    private void BreakComponent()
    {
        broken = true;
        shipComponentMeshController.ChangeMeshToBroken();
    }

    public void RepaireComponent()
    {
        broken = false;
        health = maxHealth;
        shipComponentMeshController.ChangeMeshToWorking();
    }

    public void ActivateShield(Shield shield)
    {
        this.shield = shield;
        shield.OnShieldDestroyed.AddListener(ResetShield);
    }

    private void ResetShield()
    {
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
}
