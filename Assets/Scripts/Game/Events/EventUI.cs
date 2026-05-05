using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public TextMeshProUGUI EventText;
    public List<Button> EventButtons;
    public Image EventImage;

    public CanvasGroup selfGroup => GetComponent<CanvasGroup>();
    public Image selfImage;
    private float y;
    private float offset_distance = 600;

    private void Start()
    {
        selfImage = GetComponent<Image>();
        y = selfImage.transform.position.y;
    }

    public void SetVisible()
    {
        visible = true;
        MoveUp();
    }

    public bool visible = true;

    // Move the overlay up / down to reveal the background
    public void ToggleVisible()
    {
        if (visible)
        {
            MoveDown();
        }
        else
        {
            MoveUp();
        }

        visible = !visible;
    }

    private void MoveDown()
    {
        selfImage.transform.DOMoveY(y - offset_distance, 0.2f);
    }

    private void MoveUp()
    {
        selfImage.transform.DOMoveY(y, 0.2f);
    }
}
