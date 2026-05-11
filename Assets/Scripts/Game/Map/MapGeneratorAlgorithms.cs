using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class just for map generation
/// </summary>
public static class MapGeneratorAlgorithms
{
    /// the main map generation function
    public static MapGraph GenerateMap(int stages, int pathCount, MapNodeProbabilities probabilities)
    {
        MapGraph graph = new();

        MapNode start = new()
        {
            id = 0,
            position = new Vector2(0, 0),
            type = NodeType.Rest,
            depth = 0
        };

        graph.nodes.Add(start);
        graph.startNode = start;

        List<List<MapNode>> layers = new();

        for (int i = 0; i <= stages; i++)
            layers.Add(new List<MapNode>());

        layers[0].Add(start);

        List<MapNode> previousLayer = new() { start };

        for (int stage = 1; stage <= stages; stage++)
        {
            List<MapNode> currentLayer = new();

            int nodeCount = Random.Range(pathCount, pathCount + 1);

            for (int i = 0; i < nodeCount; i++)
            {
                MapNode node = new()
                {
                    id = graph.nodes.Count,
                    position = new Vector2(stage, i),
                    depth = stage,
                    type = GetRandomType(stage/*, stages*/, probabilities)
                };

                graph.nodes.Add(node);
                currentLayer.Add(node);

                int connections = Random.Range(1, 3);

                for (int c = 0; c < connections; c++)
                {
                    int offset = Random.Range(-1, 2); // -1, 0, +1
                    int index = Mathf.Clamp(i + offset, 0, previousLayer.Count - 1);

                    var prev = previousLayer[index];

                    if (!prev.connections.Contains(node))
                        prev.connections.Add(node);
                }
            }

            previousLayer = currentLayer;
            layers[stage] = currentLayer;
        }


        MapNode boss = new()
        {
            id = graph.nodes.Count,
            position = new Vector2(stages + 1, 0),
            depth = stages + 1,
            type = NodeType.Boss
        };

        graph.nodes.Add(boss);

        foreach (var node in previousLayer)
            node.connections.Add(boss);

        FixDeadEnds(layers);

        return graph;
    }

    private static NodeType GetRandomType(int stage,/* int maxStage, */MapNodeProbabilities probabilities)
    {
       /* if (stage == maxStage)
            return NodeType.Elite;
       */
        float roll = Random.value;
        //Might be worth placing the logic in probabilities
        if (roll < probabilities.combatProbability) return NodeType.Combat;
        if (roll < probabilities.combatProbability + probabilities.eventProbability) return NodeType.Event;
        if (roll < probabilities.combatProbability + probabilities.eventProbability + probabilities.eliteProbability) return NodeType.Elite;

        return NodeType.Rest;
    }

    private static void FixDeadEnds(List<List<MapNode>> layers)
    {
        for (int stage = 0; stage < layers.Count - 1; stage++)
        {
            var currentLayer = layers[stage];
            var nextLayer = layers[stage + 1];

            if (nextLayer.Count == 0)
                continue;

            for (int i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];

                if (node.connections == null || node.connections.Count == 0)
                {
                    int index = Mathf.Clamp(i, 0, nextLayer.Count - 1);
                    node.connections.Add(nextLayer[index]);
                }
            }
        }
    }
}
