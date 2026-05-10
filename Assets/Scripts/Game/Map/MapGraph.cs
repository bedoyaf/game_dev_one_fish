using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main datastructure to store the map itself
/// Contains a list of nodes and the start node
/// </summary>
public class MapGraph
{
    public List<MapNode> nodes = new();
    public MapNode startNode;
}
