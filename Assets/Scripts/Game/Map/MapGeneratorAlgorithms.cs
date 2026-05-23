using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class just for map generation
/// </summary>
public static class MapGeneratorAlgorithms
{
    private static List<NodeType> possibleTypes = new List<NodeType> { NodeType.Combat, NodeType.Event, NodeType.Rest, NodeType.Elite};

    /// the main map generation function
    public static MapGraph GenerateMap(int stages, int pathCount, int minConnections, int maxConnections, MapNodeProbabilities probabilities)
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

        // Generate the map first
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

                int connections = Random.Range(minConnections, maxConnections + 1);

                for (int c = 0; c < connections; c++)
                {
                    int offset = Random.Range(-1, 2); // -1, 0, +1
                    int index = Mathf.Clamp(i + offset, 0, previousLayer.Count - 1);

                    var prev = previousLayer[index];

                    if (!prev.connections.Contains(node)) {
                        prev.connections.Add(node);
                        node.backConnections.Add(prev);
                    }
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

        foreach (var node in previousLayer) {
            node.connections.Add(boss);
            boss.backConnections.Add(node);
        }

        FixDeadEnds(layers);

        // Create constraints
        List<Constraint> constraints = new List<Constraint>();
        constraints.Add(new UniformityConstraint(NodeType.Elite, 1));
        constraints.Add(new UniformityConstraint(NodeType.Rest, 1));
        constraints.Add(new UniformityConstraint(NodeType.Combat, 2));
        constraints.Add(new UniformityConstraint(NodeType.Event, 2));

        constraints.Add(new MinStageConstraint(NodeType.Elite, 2));
        constraints.Add(new MinStageConstraint(NodeType.Rest, 2));

        // Now select node types propertly
        for (int stage = 1; stage < layers.Count; stage++) {
            var currentLayer = layers[stage];

            for (int i = 0; i < currentLayer.Count; i++) {
                var node = currentLayer[i];
                var possibitities = new List<NodeType>(possibleTypes);

                foreach (var constraint in constraints) {
                    constraint.ApplyConstraint(node, possibitities, stage);
                }

                node.type = GetRandomTypeFromList(possibitities, probabilities);
            }
        }

        return graph;
    }


    private static void FixDeadEnds(List<List<MapNode>> layers) {
        for (int stage = 0; stage < layers.Count - 1; stage++) {
            var currentLayer = layers[stage];
            var nextLayer = layers[stage + 1];

            if (nextLayer.Count == 0)
                continue;

            for (int i = 0; i < currentLayer.Count; i++) {
                var node = currentLayer[i];

                if (node.connections == null || node.connections.Count == 0) {
                    int index = Mathf.Clamp(i, 0, nextLayer.Count - 1);
                    node.connections.Add(nextLayer[index]);
                    nextLayer[index].backConnections.Add(node);
                }
            }
        }
    }

    /// <summary>
    /// Selects randomly from list of possibilities
    /// </summary>
    private static NodeType GetRandomTypeFromList(List<NodeType> possibilities, MapNodeProbabilities probabilities) {
        float sum = 0;
        foreach (var type in possibilities) {
            sum += GetPropabilityForType(type, probabilities);
        }

        float rand = Random.Range(0, sum - 0.01f);

        foreach (var type in possibilities) {
            rand -= GetPropabilityForType(type, probabilities);
            if (rand <= 0)
                return type;
        }

        return NodeType.Boss; // Never happens
    }

    private static float GetPropabilityForType(NodeType type, MapNodeProbabilities probabilities) {
        return type switch {
            NodeType.Combat => probabilities.combatProbability,
            NodeType.Elite => probabilities.eliteProbability,
            NodeType.Event => probabilities.eventProbability,
            NodeType.Rest => probabilities.restProbability,
            _ => 0,
        };
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

    private abstract class Constraint {
        public abstract void ApplyConstraint(MapNode node, List<NodeType> possibleNodes, int stage);
    }

    /// <summary>
    /// x nodes of same type cannot be after each other
    /// </summary>
    private class UniformityConstraint : Constraint {
        private NodeType targetType;
        private int maxCount;

        public UniformityConstraint(NodeType targetType, int maxCount) {
            this.targetType = targetType;
            this.maxCount = maxCount;
        }


        public override void ApplyConstraint(MapNode node, List<NodeType> possibleNodes, int stage) {
            if (possibleNodes.Contains(targetType)) {
                if (FindPath(node, 0)) {
                    possibleNodes.Remove(targetType);
                }
            }
        }

        // Recursive search
        private bool FindPath(MapNode node, int depth) {
            if (depth == maxCount && node.type == targetType) {
                return true;
            }

            foreach (var previous in node.backConnections) {
                if (previous.type == targetType) {
                    bool result = FindPath(previous, depth + 1);
                    if (result) return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The type cannot be in first x layers/stages
    /// </summary>
    private class MinStageConstraint : Constraint {
        private NodeType targetType;
        private int minStage;

        public MinStageConstraint(NodeType targetType, int minStage) {
            this.targetType = targetType;
            this.minStage = minStage;
        }


        public override void ApplyConstraint(MapNode node, List<NodeType> possibleNodes, int stage) {
            if (stage < minStage) {
                if (possibleNodes.Contains(targetType))
                    possibleNodes.Remove(targetType);
            }
        }
    }
}
