using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{ 

    [SerializeField]
    private Image hoverImage;

    void Start()
    {
        hoverImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        hoverImage.gameObject.SetActive(false);
    }
}
