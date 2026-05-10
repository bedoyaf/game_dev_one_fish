using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MapNodeButton : MonoBehaviour
{
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Image background;

    private MapNode node;
    private Action<MapNode> onClick;

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