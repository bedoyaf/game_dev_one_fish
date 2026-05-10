using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// Node of the map, in terms of the UI, contains a button and background image, also handles mouse hover effects and click events
/// </summary>
public class MapNodeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Image background;

    private MapNode node;
    private Action<MapNode> onClick;

    public UnityEngine.UI.Image icon;

    public void Init(MapNode node, Action<MapNode> onClick)
    {
        this.node = node;
        this.onClick = onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(node));
    }

    public void SetTypeColor(Color c)
    {
        background.color = c;
    }

    public void SetState(float alpha, Color tint)
    {
        var col = background.color;
        col = tint;
        col.a = alpha;
        background.color = col;
    }

    public void SetIcon(Sprite sprite)
    {
        if (sprite == null)
        {
            icon.color = new Color(1, 1, 1, 0);
            return;
        }

        icon.sprite = sprite;
        icon.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1.15f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1f, 0.15f);
    }
}