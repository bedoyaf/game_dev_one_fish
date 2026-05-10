using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public event Action<MapNode> OnNodeClicked;

    [SerializeField] private Vector2 nodeOffset = new Vector2( 0, 0);
    [SerializeField] private float randomNodeOffset = 10;

    [Header("Prefabs")]
    [SerializeField] private MapNodeButton nodePrefab;

    [Header("Parent")]
    [SerializeField] private RectTransform nodeParent;

    private Dictionary<MapNode, MapNodeButton> spawnedNodes = new();

    public CanvasGroup canvasGroup;

    [SerializeField] private MapUILine linePrefab;
    private List<MapUILine> lines = new();

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.2f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.DOFade(0f, 0.2f)
            .OnComplete(() => gameObject.SetActive(false));

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowGraph(MapGraph graph)
    {
        Clear();

        foreach (var node in graph.nodes)
        {
            var ui = Instantiate(nodePrefab, nodeParent);

            ui.transform.localPosition = new Vector3(
                node.position.x * 150f + nodeOffset.x+ UnityEngine.Random.Range(-randomNodeOffset,randomNodeOffset),
                node.position.y * -150f + nodeOffset.y + UnityEngine.Random.Range(-randomNodeOffset, randomNodeOffset),
                0
            );

            ui.Init(node, HandleNodeClicked);

            ui.transform.DOPunchScale(Vector3.one * 0.05f, 0.3f);

            spawnedNodes[node] = ui;
        }

        DrawConnections();

    }
    private void HandleNodeClicked(MapNode node)
    {
        OnNodeClicked?.Invoke(node);
    }

    public void UpdatePlayerPosition(MapNode current)
    {
        Debug.Log("Updating nodes");
        foreach (var kv in spawnedNodes)
        {
            var node = kv.Key;
            var ui = kv.Value;

            // 1. základní barva podle typu
            Color baseColor = GetTypeColor(node.type);

            // 2. STAV
            if (node == current)
            {
                ui.SetTypeColor(Color.yellow);
                continue;
            }

            if (node.visited)
            {
                ui.SetState(1f, baseColor * 0.6f);
                continue;
            }

            if (IsReachable(current, node))
            {
                ui.SetState(1f, baseColor); // plná barva
            }
            else
            {
                ui.SetState(0.25f, Color.gray); // šedá + transparent
            }
        }
    }

    private Color GetTypeColor(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => new Color(0.9f, 0.2f, 0.2f),
            NodeType.Elite => new Color(0.7f, 0.2f, 0.9f),
            NodeType.Event => new Color(0.2f, 0.5f, 0.9f),
            NodeType.Shop => new Color(0.2f, 0.9f, 0.4f),
            NodeType.Boss => new Color(1f, 0.8f, 0.2f),
            _ => Color.white
        };
    }

    private bool IsReachable(MapNode current, MapNode target)
    {
        return current.connections.Contains(target);
    }
    private void Clear()
    {
        foreach (var n in spawnedNodes.Values)
            Destroy(n.gameObject);

        spawnedNodes.Clear();
    }

    private void DrawConnections()
    {
        ClearLines();

        foreach (var node in spawnedNodes.Keys)
        {
            foreach (var target in node.connections)
            {
                // bezpečnost
                if (!spawnedNodes.ContainsKey(target))
                    continue;

                var line = Instantiate(linePrefab, nodeParent);

                Vector2 a = spawnedNodes[node].GetComponent<RectTransform>().anchoredPosition;
                Vector2 b = spawnedNodes[target].GetComponent<RectTransform>().anchoredPosition;

                line.SetPositions(a, b);

                lines.Add(line);
            }
        }
    }

    private void ClearLines()
    {
        foreach (var l in lines)
            Destroy(l.gameObject);

        lines.Clear();
    }
}