using DG.Tweening;
using TMPro;
using UnityEngine;

public class InventoryUIScript : MonoBehaviour
{

    [SerializeField]
    private CanvasGroup parentTransform;

    [SerializeField]
    private TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        var man = GameManager.Instance.currentGameplayManager;

        // Show / Hide only when in combat and when holding something
        var vis = man.InCombat && man.rewardController.CurrentlyHolding > 0;
        parentTransform.alpha = vis ? 1 : 0;

        // Update the text for how many actually holding
        text.text = $"Components Stolen: {man.rewardController.CurrentlyHolding}/{man.rewardController.inventoryCapacity}";
    }
}
