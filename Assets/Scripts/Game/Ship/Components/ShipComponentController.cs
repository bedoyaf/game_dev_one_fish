using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
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
    public bool activated = false;

    public int requiredEnergy = 0;

    // how much currency added if destroyed by the player
    public int destroyRevenue = 1;

    private IShipComponentBehaviour componentBehaviour;

    public ShipController shipController { get; private set; }

    public GameObject ComponentMesh; // The child of the component, that has the mesh on it
    public GameObject ComponentHitbox; // The child of the component that has the hitbox on it
    
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

            AudioManager.Instance.PlaySFX(activationClip, transform.position);
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
    }


    public void RepairFromComponent()
    {

    }

    public void RepairClick()
    {
        // TODO: maybe better repair cost
        if (health != maxHealth)
        {
            Debug.Log("Can repair");
            if (shipController.UseCurrency(1))
            {
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
                Debug.Log("Can't repair, can't afford");
            }
        } else
        {
            Debug.Log("Can't repair, full health");
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
