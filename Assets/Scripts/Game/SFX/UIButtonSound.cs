using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private UISoundData clickSound;
    [SerializeField] private UISoundData hoverSound; // TODO use

    public void OnPointerClick(PointerEventData eventData) {
        PlaySound(clickSound);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        PlaySound(hoverSound);
    }

    private void Awake() {
        //button = GetComponent<Button>();
        //if (button != null) {
        //    button.onClick.AddListener(() => PlaySound(clickSound));
        //}
    }

    private void PlaySound(UISoundData sound) {
        if (sound == null) return;

        AudioManager.Instance.PlayUISound(sound);
    }
}
