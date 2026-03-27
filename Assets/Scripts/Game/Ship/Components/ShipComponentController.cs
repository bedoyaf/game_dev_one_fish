using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using static ComponentGrid;


public class ShipComponentController : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent OnDeath;
    public bool activated = false;
    [SerializeField] private bool requiresPower = true;
    public bool poweredOn = false;

    public ComponentPlacementRules placementRules;

    public ShipController shipController { get; private set; }

    private IShipComponentBehaviour componentBehaviour;

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

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ActivateComponent()
    {
        if(!activated)
        {
            componentBehaviour.OnActivate();
            activated = true;
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
        Destroy(gameObject);
    }

    [Serializable]
    public class ComponentPlacementRules {
        public int Width = 1;
        public int Height = 1;

        public int Top;
        public int Right;
        public int Bottom;
        public int Left;

        public bool blockSurroundings => Top != 0 || Right != 0 || Bottom != 0 || Left != 0;

        public ComponentGridTile connectedTile; 
    }
}
