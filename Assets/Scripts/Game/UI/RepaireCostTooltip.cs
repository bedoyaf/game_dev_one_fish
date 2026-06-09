using UnityEngine;

using TMPro;

public class RepairCostTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Vector3 offset= new Vector3(20, -20, 0);

    private Camera uiCamera;

    // Do not use...
    // When this is called, it won't be actually done
    // Until the object is active in the scene
    // So when it gets shown for the first time
    // this will trigger and hide it immediately
    private void Awake()
    {
        // Hide();
    }

    private void Update()
    {
        root.position = Input.mousePosition + offset;
    }

    public void Show(int cost)
    {
        costText.text = $"{cost}";
        root.gameObject.SetActive(true);
    }

    public void Hide()
    {
        root.gameObject.SetActive(false);
    }
}