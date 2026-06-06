using UnityEngine;

using TMPro;

public class RepairCostTooltip : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Vector3 offset= new Vector3(20, -20, 0);

    private Camera uiCamera;

    private void Awake()
    {
        Hide();
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