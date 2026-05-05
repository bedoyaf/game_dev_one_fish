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

    [SerializeField] private GameplayFlowManager gameplayFlowManager;


    [SerializeField, ReadOnly] private List<MapChoices> choices = new();
    [SerializeField] private int currentStage = 0; // Should be readonly
    [SerializeField] private float currentDifficulty = 0; // Should be readonly

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
        instantiatedUI.SetVisible();

        // Select stages
        if (currentStage == bossStage) {
            choices.Add(MapChoices.Boss);
        }
        else {
            choices.Add(MapChoices.Combat); // Combat is always there
            if (Random.Range(0, 2) == 0)
                choices.Add(MapChoices.Elite);
            //else
            choices.Add(MapChoices.Event);
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
    /// TODO select actual enemy.
    /// </summary>
    /// <param name="choice"></param>
    public void OnButtonClick(int choice) {
        var choiceData = new MapChoiceData();
        switch (choices[choice]) {
            case MapChoices.Combat:
                // Select random enemy based on difficulty (with a chance to select a bit harder or easier one)
                //int difficulty = (int)currentDifficulty;
                //var enemy = GetEnemyFromDifficulty(difficulty);
                choiceData.fight = true;
                choiceData.difficulty = (int)currentDifficulty;
                break;


            case MapChoices.Elite:
                //var eliteEnemy = GetEnemyFromDifficulty((int)currentDifficulty + 3);
                choiceData.fight = true;
                choiceData.difficulty = (int)currentDifficulty + 3;

                break;
            case MapChoices.Event:
                choiceData.fight = false;

                break;
            case MapChoices.Boss:
                //var boss = bosses.GetRandom();
                choiceData.fight = true;
                choiceData.boss = true;

                break;
            default:
                Debug.LogError("Unknown map choice selected!");
                break;
        }

        instantiatedUI.gameObject.SetActive(false);
        gameplayFlowManager.CloseMapController(choiceData);
    }

    public void OnRepairButtonClicked()
    {
        instantiatedUI.gameObject.SetActive(false);
        gameplayFlowManager.EnterRepairsMode();
    }

    public struct MapChoiceData {
        public bool fight; // If false, it means event
        public bool boss; // If true, the fight is boss fight
        public int difficulty; // The difficulty the fight should have
    }

    /// <summary>
    /// Instantiates the UI and binds the buttons.
    /// </summary>
    private void InitializeUI() {
        instantiatedUI = Instantiate(mapUIPrefab, canvas.transform);
        // Move, so that the pause menu is always in front
        instantiatedUI.transform.SetAsFirstSibling();
        for (int i = 0; i < instantiatedUI.MapButtons.Count; i++) {
            int a = i;
            instantiatedUI.MapButtons[i].onClick.AddListener(() => OnButtonClick(a));
        }

        instantiatedUI.TEMP_RepairButton.onClick.AddListener(OnRepairButtonClicked);
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