using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RewardController : MonoBehaviour
{
    public List<ShipComponentController> storedComponents = new List<ShipComponentController>(); //prefaby

    private List<ShipComponentController> currentChoices = new List<ShipComponentController>();
    private List<ShipComponentController> pool = new List<ShipComponentController>();

    [SerializeField] private int choicesToShow = 9;
    [SerializeField] private int picksNeeded = 3;


    public int inventoryCapacity = 3;
    public int CurrentlyHolding => storedComponents.Count;



    [SerializeField] private GameplayFlowManager flowManager;

    private bool choosing = false;

    // ----------------------------------------------------

    public void StartChoosing(ShipController shipController)
    {
        pool = shipController.componentGrid.GetAllComponents()
            .Where(c => !c.broken)
            .ToList();

        for(int i =0;i<pool.Count;i++)
        {
            if (pool[i].componentType == ShipComponentController.ComponentType.MainCabin) 
            {
                pool.RemoveAt(i);
                break;
            }
        }

        storedComponents.Clear();

        choosing = true;
        GenerateChoices();
    }

    public void ClearStoredComponents()
    {
        storedComponents.Clear();
    }

    // ----------------------------------------------------

    void GenerateChoices()
    {
        currentChoices.Clear();

        var tempPool = new List<ShipComponentController>(pool);

        for (int i = 0; i < choicesToShow && tempPool.Count > 0; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            currentChoices.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }
    }

    // ----------------------------------------------------

    void PickComponent(ShipComponentController comp)
    {
        storedComponents.Add(comp.ComponentPrefab);
        pool.Remove(comp);

        if (storedComponents.Count >= picksNeeded || pool.Count<=0)
        {
            choosing = false;
            Debug.Log("DONE PICKING");
            flowManager.OnShowRewardEnd();
            return;
        }

        GenerateChoices();
    }

    public void AssignComponent(ShipComponentController comp) {
        storedComponents.Clear();
        storedComponents.Add(comp.ComponentPrefab);
    }

    public void AppendComponent(ShipComponentController comp)
    {
        storedComponents.Add(comp.ComponentPrefab);
    }

    // ----------------------------------------------------

    void OnGUI()
    {
        if (!choosing) return;

        float size = 100;
        float padding = 10;

        for (int i = 0; i < currentChoices.Count; i++)
        {
            int row = i / 3;
            int col = i % 3;

            float x = 200 + col * (size + padding);
            float y = 200 + row * (size + padding);

            var comp = currentChoices[i];

            string label = comp.componentName;

            if (GUI.Button(new Rect(x, y, size, size), label))
            {
                PickComponent(comp);
            }
        }

        GUI.Label(new Rect(10, Screen.height - 30, 300, 30),
            $"Picked: {storedComponents.Count} / {picksNeeded}");
    }
}