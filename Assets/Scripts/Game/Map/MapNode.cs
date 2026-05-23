using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure, represents a single node on the map, contains its position, type and connections to other nodes
/// </summary>
public class MapNode
{
    public int id;
    public Vector2 position; // pro UI mapu
    public NodeType type;

    public List<MapNode> connections = new();
    public List<MapNode> backConnections = new(); // Edge back in the graph
    public bool visited;
    public int depth;
}
public enum NodeType
{
    Combat,
    Elite,
    Event,
   // Shop,
    Rest,
    Boss
}