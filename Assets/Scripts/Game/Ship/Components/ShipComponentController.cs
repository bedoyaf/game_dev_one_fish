using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Takes energy through the component system, makes sure it fits the batteries. Returns true false if it had enaugh
/// </summary>
public class ShipComponentController : MonoBehaviour
{
    public int health = 10;
    public UnityEvent OnDeath;
    public bool activated = false;
    [SerializeField] private bool requiresPower = true;
    public bool poweredOn = false;

    private IShipComponentBehaviour componentBehaviour;

    [Header("Builder stuff")]
    public ComponentPlacement placementRules;

    public ShipController shipController { get; private set; }

    public ShipComponentController componentPrefab; //TODO, might be useless delete these
    public GameObject ComponentMesh; // The child of the component, that has the mesh on it
    
    private Shield shield;

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
        if(!activated && componentBehaviour!=null)
        {
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

    private void Die()
    {
        OnDeath?.Invoke();

        // Destroys the component and works with the grid.
        shipController.componentGrid.OnComponentDeath(placementRules.connectedTile);
        
        //Destroy(gameObject);
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
        public int Width = 1;
        public int Height = 1;

        public int Top;
        public int Right;
        public int Bottom;
        public int Left;

        public bool blockSurroundings => Top != 0 || Right != 0 || Bottom != 0 || Left != 0;

        public ComponentGridTile connectedTile; // TODO - sorry, does not belong here
    }
}
