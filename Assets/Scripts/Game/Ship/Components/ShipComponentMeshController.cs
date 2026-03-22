using UnityEngine;

public class ShipComponentMeshController : MonoBehaviour
{
    [SerializeField] private ShipComponentController shipComponentController;

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
            shipComponentController.ActivateComponents();
        }
        else if(shipComponentController.activated)
        {
            shipComponentController.DeactivateComponents();
        }
    }
}
