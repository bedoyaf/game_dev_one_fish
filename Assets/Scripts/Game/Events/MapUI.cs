using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager of the UI maps, nodes where to place them, what icons and more
/// </summary>
public class MapUI : MonoBehaviour
{
    public event Action<MapNode> OnNodeClicked;

    [SerializeField] private Vector2 nodeOffset = new Vector2( 0, 0);
    [SerializeField] private Vector2 nodeDistances = new Vector2(150, 150);
    [SerializeField] private float randomNodeOffset = 10;

    [Header("Prefabs")]
    [SerializeField] private MapNodeButton nodePrefab;

    [Header("Parent")]
    [SerializeField] private RectTransform nodeParent;

    private Dictionary<MapNode, MapNodeButton> spawnedNodes = new();

    public CanvasGroup canvasGroup;

    // public int sightDistance = 1; // How far can the player see
    private int sightDistance = 1;
    public bool showWholeStages = true;

    [SerializeField] private MapUILine linePrefab;
    private List<MapUILine> lines = new();

    [SerializeField] private Color currentNodeColor = Color.yellow;
    [SerializeField] private Color combatNodeColor= new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color eliteNodeColor = new Color(0.7f, 0.2f, 0.9f);
    [SerializeField] private Color eventNodeColor = new Color(0.2f, 0.5f, 0.9f);
    [SerializeField] private Color restNodeColor = Color.green;
    [SerializeField] private Color bossNodeColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color defaultNodeColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color visitedNodeColor = Color.gray;
    [SerializeField] private Color visibleUnreachableNodeColor = Color.gray;

    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite combatSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite bossSprite;

    [SerializeField] private bool debugAllVisible;

    /// <summary>
    /// function to hide and show the map ui
    /// </summary>
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

    /// <summary>
    /// Function to setup the map itself, mainly connections and the nodes
    /// </summary>
    public void ShowGraph(MapGraph graph)
    {
        Clear();

        foreach (var node in graph.nodes)
        {
            var ui = Instantiate(nodePrefab, nodeParent);

            ui.transform.localPosition = new Vector3(
                node.position.x * nodeDistances.x + nodeOffset.x+ UnityEngine.Random.Range(-randomNodeOffset,randomNodeOffset),
                node.position.y * -nodeDistances.y + nodeOffset.y + UnityEngine.Random.Range(-randomNodeOffset, randomNodeOffset),
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

    /// <summary>
    /// function updates the visual of the map during movement
    /// </summary>
    public void UpdatePlayerPosition(MapNode current, int sightDistance)
    {
        this.sightDistance = sightDistance;
        if (debugAllVisible) sightDistance = 10000;
        Debug.Log("Updating nodes");
        foreach (var kv in spawnedNodes)
        {
            var node = kv.Key;
            var ui = kv.Value;

            Color baseColor = GetTypeColor(node.type);

            if (node == current)
            {
                ui.SetIcon(playerSprite);
                ui.SetTypeColor(currentNodeColor);
                continue;
            }

            if (node.visited)
            {
                ui.SetState(1f, visitedNodeColor);
                ui.SetIcon(null);
                continue;
            }

            if (IsReachable(current, node, sightDistance))
            {
                ui.SetIcon(GetTypeIcon(node.type));
                ui.SetState(1f, baseColor);
                ui.SetReachable(IsReachable(current, node, 1), baseColor, 0.25f);
            }
            else
            {
                ui.SetIcon(null);
                ui.SetState(0.25f, defaultNodeColor); 
            }
        }
    }

    private Color GetTypeColor(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => combatNodeColor,
            NodeType.Elite => eliteNodeColor,
            NodeType.Event => eventNodeColor,
            NodeType.Boss => bossNodeColor,
            NodeType.Rest => restNodeColor, 
            _ => Color.white
        };
    }

    public Sprite GetTypeIcon(NodeType type)
    {
        return type switch
        {
            NodeType.Combat => combatSprite,
            NodeType.Elite => eliteSprite,
            NodeType.Event => eventSprite,
            NodeType.Boss => bossSprite,
            NodeType.Rest => restSprite, 
            _ => combatSprite
        };
    }

    private bool IsReachable(MapNode current, MapNode target, int sightDistance)
    {
        if (sightDistance <= 1)
            return current.connections.Contains(target);
        else {
            if (showWholeStages)
                return target.depth > current.depth && target.depth - current.depth <= sightDistance;
            else {
                // BFS in the left direction - gets really what is in from of the player
                var previousNodes = target.backConnections;
                for (int i = 0; i < sightDistance; i++) {
                    var newPrevious = new HashSet<MapNode>();

                    foreach (var prev in previousNodes) {
                        if (prev == current) return true;

                        foreach (var prevPrev in prev.backConnections) {
                            newPrevious.Add(prevPrev);
                        }
                    }

                    previousNodes = newPrevious.ToList();
                }
            }

            return false;
        }
    }
    private void Clear()
    {
        foreach (var n in spawnedNodes.Values)
            Destroy(n.gameObject);

        spawnedNodes.Clear();
    }

    /// <summary>
    /// functions for drawing lines between the nodes
    /// </summary>
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