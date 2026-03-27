using UnityEngine;

public class ShipComponentMeshController : MonoBehaviour
{
    private ShipComponentController shipComponentController;

    public void Awake()
    {
        if(shipComponentController == null)shipComponentController = GetComponentInParent<ShipComponentController>();
        if (shipComponentController == null) Debug.LogError("ShipComponentMeshController lacks the parent script");
    }
    public void OnMouseClick()
    {
        Debug.Log("Component has been clicked");
        if(!shipComponentController.activated)
        {
            shipComponentController.ActivateComponent();
        }
        else if(shipComponentController.activated)
        {
            shipComponentController.DeactivateComponent();
        }
    }

    public void OnDamagableCollision(float amount)
    {
        shipComponentController.TakeDamage(amount);
    }
}
