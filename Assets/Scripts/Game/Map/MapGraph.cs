using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main datastructure to store the map itself
/// Contains a list of nodes and the start node
/// Includes visited state tracking for save/load functionality
/// </summary>
[System.Serializable]
public class MapGraph
{
    public List<MapNode> nodes = new();
    public MapNode startNode;

    /// <summary>
    /// Stores which nodes have been visited (by node ID)
    /// Used for save/load to persist player progress on the map
    /// </summary>
    [SerializeField]
    public List<int> visitedNodeIds = new();

    /// <summary>
    /// Saves the visited state of all nodes to the visitedNodeIds list
    /// Call this before saving to JSON
    /// </summary>
    public void SaveVisitedState()
    {
        visitedNodeIds.Clear();
        foreach (var node in nodes)
        {
            if (node.visited)
            {
                visitedNodeIds.Add(node.id);
            }
        }
    }

    /// <summary>
    /// Loads the visited state from visitedNodeIds list back to nodes
    /// Call this after loading from JSON
    /// </summary>
    public void LoadVisitedState()
    {
        foreach (var node in nodes)
        {
            node.visited = visitedNodeIds.Contains(node.id);
        }
    }
}
