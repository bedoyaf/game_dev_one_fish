using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

public class MapNode
{
    public int id;
    public Vector2 position; // pro UI mapu
    public NodeType type;

    public List<MapNode> connections = new();
    public bool visited;
    public int depth;
}
public enum NodeType
{
    Combat,
    Elite,
    Event,
    Shop,
    Rest,
    Boss
}