using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public TextMeshProUGUI MapText;
    public List<Button> MapButtons;
    public Image MapImage;

    public Button TEMP_RepairButton;

    private Image selfImage;
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
        if(visible)
        {
            MoveDown();
        } else
        {
            MoveUp();
        }

            visible = !visible;
    }

    private void MoveDown()
    {
        if (selfImage != null)
            selfImage.transform.DOMoveY(y - offset_distance, 0.2f);
    }

    private void MoveUp()
    {
        if(selfImage != null)
            selfImage.transform.DOMoveY(y, 0.2f);
    }
}
