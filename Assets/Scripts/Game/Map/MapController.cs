using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Displays possible choices to the player.
/// </summary>
public class MapController : MonoBehaviour
{
    /// <summary>
    /// List of all the enemies the player can meet
    /// </summary>
    public List<ShipData> enemies;

    /// <summary>
    /// List of all possible bosses
    /// </summary>
    public List<ShipData> bosses;

    /// <summary>
    /// When the boss battle happens
    /// </summary>
    public int bossStage = 10;

    [SerializeField, ReadOnly] private List<MapChoices> choices = new();
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float currentDifficulty = 0;

    [Header("UI")]
    public Canvas canvas;
    public MapUI mapUIPrefab;
    private MapUI instantiatedUI;

    /// <summary>
    /// Displays few choices on the screen.
    /// </summary>
    /// <param name="currentDifficulty">How hard the enemies should be</param>
    public void DisplayChoices() {
        currentStage++;
        currentDifficulty++; // NOTE - here you can change difficulty scaling
        choices.Clear();

        if (instantiatedUI == null) {
            InitializeUI();
        }

        instantiatedUI.gameObject.SetActive(true);

        // Select stages
        if (currentStage == bossStage) {
            choices.Add(MapChoices.Boss);
        }
        else {
            choices.Add(MapChoices.Combat); // Combat is always there
            if (Random.Range(0, 2) == 0) {
                choices.Add(MapChoices.Elite);
            }
            else {
                choices.Add(MapChoices.Event);
            }
        }

        // Show the choices
        for(int i = 0; i < choices.Count; i++) {
            instantiatedUI.MapButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = GetDescription(choices[i]);
            instantiatedUI.MapButtons[i].gameObject.SetActive(true);
        }

        // Hide rest of buttons
        for (int i = choices.Count; i < instantiatedUI.MapButtons.Count; i++) {
            instantiatedUI.MapButtons[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Act according to what user chose.
    /// </summary>
    /// <param name="choice"></param>
    public void OnButtonClick(int choice) {
        switch (choices[choice]) {
            case MapChoices.Combat:
                // Select random enemy based on difficulty (with a chance to select a bit harder or easier one)
                int difficulty = (int)currentDifficulty;
                var enemy = GetEnemyFromDifficulty(difficulty);
                //GameManager.Instance.currentGameplayManager.Fight(enemy)
                break;


            case MapChoices.Elite:
                var eliteEnemy = GetEnemyFromDifficulty((int)currentDifficulty + 3);
                //GameManager.Instance.currentGameplayManager.Fight(enemy)


                break;
            case MapChoices.Event:
                //GameManager.Instance.currentGameplayManager.SwitchToEvent();

                break;
            case MapChoices.Boss:
                var boss = bosses.GetRandom();
                //GameManager.Instance.currentGameplayManager.BossFight(enemy)

                break;
            default:
                Debug.LogError("Unknown map choice selected!");
                break;
        }

        instantiatedUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Instantiates the UI and binds the buttons.
    /// </summary>
    private void InitializeUI() {
        instantiatedUI = Instantiate(mapUIPrefab, canvas.transform);
        for(int i = 0; i < instantiatedUI.MapButtons.Count; i++) {
            int a = i;
            instantiatedUI.MapButtons[i].onClick.AddListener(() => OnButtonClick(a));
        }
    }

    /// <summary>
    /// Gets enemy from give difficulty. Has a small chance to select a bit easier or harder enemy.
    /// If no enemies from +-1 range of difficulty exists, choose first easier one.
    /// </summary>
    private ShipData GetEnemyFromDifficulty(int difficulty) {
        if (enemies.Count == 0) return null;

        var easierEnemies = enemies.FindAll(x => x.enemyDifficulty == difficulty - 1);
        var harderEnemies = enemies.FindAll(x => x.enemyDifficulty == difficulty + 1);
        var normalEnemies = enemies.FindAll(x => x.enemyDifficulty == difficulty - 1);
        var enemyLists = new List<List<ShipData>>() {
                    easierEnemies, harderEnemies, normalEnemies, normalEnemies, normalEnemies
                };
        enemyLists.Shuffle();

        // Select random enemy
        foreach (var list in enemyLists) {
            if (list.Count > 0) {
                return list.GetRandom();
            }
        }

        // Selects first easier enemy.
        Debug.LogError("No enemy found for set difficulty.");
        int n = -1;
        for(int i = 0; i < enemies.Count; i++) {
            if (enemies[i].enemyDifficulty < difficulty) {
                break;
            }
            n++;
        }
        n = Mathf.Clamp(n, 0, enemies.Count - 1);
        return enemies[n];
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


// https://discussions.unity.com/t/how-to-make-a-readonly-property-in-inspector/75448/5
public class ReadOnlyAttribute : PropertyAttribute {
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}