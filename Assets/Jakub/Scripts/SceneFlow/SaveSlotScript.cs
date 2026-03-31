using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotScript : MonoBehaviour
{
    [Tooltip("The save slot this instance points to")]
    [SerializeField]
    private SaveSlotData slotData;

    [Tooltip("The Text Mesh Pro instance to show info on")]
    [SerializeField]
    private TextMeshProUGUI slotInfoPanel;

    [Tooltip("The delete request button")]
    [SerializeField]
    private Button deleteRequest;

    [Tooltip("The delete confirm button")]
    [SerializeField]
    private Button confirmDeleteRequest;


    void Start()
    {
        UpdateVisuals();
    }

    private void OnEnable()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (!slotData.IsSlotEmpty)
        {
            slotInfoPanel.text = "Continue...";
            deleteRequest.gameObject.SetActive(true);
        }
        else
        {
            slotInfoPanel.text = "New Game";
            deleteRequest.gameObject.SetActive(false);
        }

        confirmDeleteRequest.gameObject.SetActive(false);
    }

    public void SelectSlot()
    {
        if (slotData.IsSlotEmpty)
        {
            slotData.CreateSlot();
        }

        // Set the active Save Slot
        GameManager_Jakub.Instance.SelectSaveSlot(slotData);
        // Start the game
        GameManager_Jakub.Instance.StartGame();
    }

    public void DeleteRequest()
    {
        confirmDeleteRequest.gameObject.SetActive(true);
    }

    public void DeleteSlot()
    {
        deleteRequest.gameObject.SetActive(false);
        confirmDeleteRequest.gameObject.SetActive(false);

        slotData.ClearSlot();
        slotInfoPanel.text = "New Game";
    }

    
}
