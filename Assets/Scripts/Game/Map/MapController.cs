using DG.Tweening;

using System.Collections.Generic;

using TMPro;

//using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Displays possible choices to the player.
/// </summary>
public class MapController : MonoBehaviour
{
    /// <summary>
    /// List of all the enemies the player can meet
    /// </summary>
    //public List<ShipData> enemies;

    /// <summary>
    /// List of all possible bosses
    /// </summary>
    //public List<ShipData> bosses;

    /// <summary>
    /// When the boss battle happens
    /// </summary>
    public int bossStage = 10;

    /// <summary>
    /// How much stronger are elite enemies than normal
    /// </summary>
    public int eliteEnemyDifficulty = 3;

    [SerializeField] private GameplayFlowManager gameplayFlowManager;


    private MapGraph graph;
    private MapNode currentNode;
    [SerializeField] private float currentDifficulty = 0; // Should be readonly

    public MapNode CurrentNode => currentNode;
    public float CurrentDifficulty => currentDifficulty;
    public float EliteDifficulty => currentDifficulty + eliteEnemyDifficulty;


    [Header("UI")]    
    public Canvas canvas;
    public MapUI mapUI;

    public void StartMap()
    {
        graph = MapGeneratorAlgorithms.GenerateMap(6, 3);

        currentNode = graph.startNode;
        currentNode.visited = true;

        mapUI.ShowGraph(graph);

        mapUI.OnNodeClicked += OnNodeSelected;
        mapUI.UpdatePlayerPosition(currentNode);
    }

    public void DisplayMap()
    {
        mapUI.Show();
        mapUI.UpdatePlayerPosition(currentNode);
    }

    private void OnNodeSelected(MapNode node)
    {
        if (!currentNode.connections.Contains(node))
            return;

        currentNode = node;
        currentNode.visited = true;

        mapUI.UpdatePlayerPosition(currentNode);
        ResolveNode(node);
        mapUI.Hide();
    }

    private void ResolveNode(MapNode node)
    {
        MapChoiceData data = new MapChoiceData();

        switch (node.type)
        {
            case NodeType.Combat:
                data.fight = true;
                data.difficulty = CalculateDifficulty(node);
                break;

            case NodeType.Elite:
                data.fight = true;
                data.difficulty = CalculateDifficulty(node) + 3;
                break;

            case NodeType.Event:
                data.fight = false;
                break;

            case NodeType.Rest:
                gameplayFlowManager.EnterRepairsMode();
                return;

            case NodeType.Boss:
                data.fight = true;
                data.boss = true;
                break;
        }

        gameplayFlowManager.CloseMapController(data);
    }

    private int CalculateDifficulty(MapNode node)
    {
        return currentNode.depth;
    }

    public void OnRepairButtonClicked()
    {
        gameplayFlowManager.EnterRepairsMode();
    }

    public struct MapChoiceData {
        public bool fight; // If false, it means event
        public bool boss; // If true, the fight is boss fight
        public int difficulty; // The difficulty the fight should have
    }

    private string GetDescription(MapChoices choice) {
        return choice switch {
            MapChoices.Combat => "Common enemy",
            MapChoices.Elite => "Elite",
            MapChoices.Event => "Event",
            MapChoices.Boss => "Boss",
            _ => "Should not happen..."
        };
    }

    public enum MapChoices {
        Combat,
        Elite,
        Event,
        Boss,
    }
}
