using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// main components manages, handles activation, damage and more
/// </summary>
public class ShipComponentController : MonoBehaviour
{
    public int maxHealth = 10;
    public int health = 10;
    public UnityEvent<ShipComponentController> OnDeath;
    public string componentName; // Name that uniquely! identifies this component 
    public bool activated = false;

    public int requiredEnergy = 0;

    private IShipComponentBehaviour componentBehaviour;

    [Header("Builder stuff")]
    public ComponentPlacement placementRules;

    public ShipController shipController { get; private set; }

    public ShipComponentController componentPrefab; //TODO, might be useless delete these
    public GameObject ComponentMesh; // The child of the component, that has the mesh on it
    
    
    public Shield shield { private set; get; }

    [HideInInspector] public ShipComponentMeshController shipComponentMeshController;
    private ComponentCooldown cooldown;

    public bool broken {  get; private set; } = false;

  //  public UnityEvent OnBroken; Ondeath preferable 

    void Start()
    {
        componentBehaviour = GetComponent<IShipComponentBehaviour>();
        if (componentBehaviour == null) Debug.LogWarning("Missing behaviour");//Make it error
        cooldown = GetComponent<ComponentCooldown>();
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

        if (shield != null)
        {
            Debug.Log("Shield catched damage");
            shield.TakeDamage(dmg);
            return;
        }

        Debug.Log("No shield -> damaging HP");

        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ActivateComponent()
    {
        if(broken) return;
        if (componentBehaviour == null) return;

        if (cooldown != null && !cooldown.IsReady)
        {
            Debug.Log("Component is on cooldown");
            return;
        }

        if (!activated)
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

    }
}
