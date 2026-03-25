using System;
using UnityEngine;
using UnityEngine.Events;
using static ComponentGrid;


public class ShipComponentController : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent OnDeath;
    public bool activated = false;

    public ComponentPlacementRules placementRules;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    public void ActivateComponents()
    {

    }

    public void DeactivateComponents()
    {

    }

    private void Die()
    {
        OnDeath?.Invoke();
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
