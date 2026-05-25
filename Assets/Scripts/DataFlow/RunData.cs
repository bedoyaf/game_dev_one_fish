using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "RunData", menuName = "Scriptable Objects/RunData")]
public class RunData : ScriptableObject
{
    // Game progression data
    public MapGraph mapGraph;
    public int map_pos;

    // Ship state (serializable snapshot of the player ship)
    public ShipState playerShipState;

    /// <summary>
    /// Saves the RunData to a JSON file
    /// Properly serializes map visited states and ship state
    /// </summary>
    public void SaveToJson(string filePath)
    {
        // Save visited state before serializing
        if (mapGraph != null)
        {
            mapGraph.SaveVisitedState();
        }

        string json = JsonUtility.ToJson(this, true);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
        Debug.Log($"RunData saved to: {filePath}\n" +
                  $"Map nodes: {mapGraph?.nodes.Count ?? 0}, " +
                  $"Visited nodes: {mapGraph?.visitedNodeIds.Count ?? 0}, " +
                  $"Player position: {map_pos}\n" +
                  $"Ship Energy: {playerShipState?.storedEnergy ?? 0}, " +
                  $"Money: {playerShipState?.storedMoney ?? 0}");
    }

    /// <summary>
    /// Loads RunData from a JSON file
    /// Restores map visited states and ship state
    /// </summary>
    public static RunData LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found at: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        RunData data = ScriptableObject.CreateInstance<RunData>();
        JsonUtility.FromJsonOverwrite(json, data);

        // Restore visited state after deserializing
        if (data.mapGraph != null)
        {
            data.mapGraph.LoadVisitedState();
        }

        Debug.Log($"RunData loaded from: {filePath}\n" +
                  $"Map nodes: {data.mapGraph?.nodes.Count ?? 0}, " +
                  $"Visited nodes: {data.mapGraph?.visitedNodeIds.Count ?? 0}, " +
                  $"Player position: {data.map_pos}\n" +
                  $"Ship Energy: {data.playerShipState?.storedEnergy ?? 0}, " +
                  $"Money: {data.playerShipState?.storedMoney ?? 0}");

        return data;
    }
}
