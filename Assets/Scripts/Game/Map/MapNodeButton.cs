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

    public bool CanClick => isReachable && !visited;

    private bool isReachable;

    public void Init(MapNode node, Action<MapNode> onClick)
    {
        this.node = node;
        this.onClick = onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(node));

    }

    public void SetTypeColor(Color c)
    {
        //background.color = c;
        icon.color = c;
    }

    bool visited = false;
    public void SetVisited(bool state=true)
    {
        visited = state;
    }

    public void SetState(float alpha, Color tint)
    {
        if(icon.sprite == null)
        {
            alpha = 0f;
        }



        var col = icon.color;
        col = tint;
        col.a = alpha;
        icon.color = col;
    }

    public void SetReachable(bool reachable, Color tint, float alpha) {
        isReachable = reachable;
        if (!reachable) {
            SetState(alpha, tint);
        }
    }

    public void SetIcon(Sprite sprite)
    {
        if (sprite == null)
        {
            // icon.color = new Color(1, 1, 1, 0);
            return;
        }

        icon.sprite = sprite;
        // icon.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        icon.transform.DOKill();
        if (CanClick)
        {
            icon.transform.DOScale(1.45f, 0.15f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        icon.transform.DOKill();
        icon.transform.DOScale(1.35f, 0.15f);
    }
}