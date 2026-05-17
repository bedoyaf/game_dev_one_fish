using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CombatController : SmartSingleton<CombatController>
{
    [SerializeField] private ShipController playerShip;
    // [SerializeField] private ShipController enemyShip;
    private ShipController currentEnemyInstance = null;

    [SerializeField] private Transform enemySpawnPosition;

    // [SerializeField] private List<ShipData> enemyShipDesigns;

    [SerializeField] private List<ShipController> enemyShipPrefabs;

    [SerializeField] private GameplayFlowManager gameplayFlowManager;


    public bool isPaused { private set; get; } = false;
    public bool combatEnded { private set; get; } = false;

    // private EnemyShipAgent enemyShipAgent;
    private EnemyShipAgent currentEnemyAI;

    public bool playerWon { get; private set; } = false;

    private bool shipGiven;

    public void Start()
    {
        currentEnemyInstance = gameplayFlowManager.EnemyShip;
    }


    // The picked ship instance / else use random
    private ShipController pickedShip = null;

    public void GenerateEnemyShip(ShipController playerShip)
    {
        if (!shipGiven)
        {
            pickedShip = enemyShipPrefabs.GetRandom();
        }

        if (currentEnemyInstance != null)
            Destroy(currentEnemyInstance.gameObject);

        currentEnemyInstance = Instantiate(pickedShip);
        currentEnemyInstance.transform.position = enemySpawnPosition.position;

        currentEnemyAI = currentEnemyInstance.gameObject.GetComponent<EnemyShipAgent>();
        
        shipGiven = false;        

        currentEnemyInstance.EnableShip();
        currentEnemyInstance.ResetShipForCombat();
        currentEnemyAI.SetPlayerShip(playerShip);

    }

    //CURRENTLY THE SHIP REPAIRS ITSELF AND RESETS, CHANGE
    private void SetupPlayerShip()
    {
        playerShip.ResetShipForCombat();
    }

    public void StopGame()
    {
        //Time.timeScale = 0f;
        //MyTime.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        //Time.timeScale = 1f;
     //   MyTime.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Returns the newly created enemy's Ship Controller.
    /// </summary>
    /// <returns></returns>
    public ShipController LoadEnemyShip(ShipController playerShip)
    {
        // enemyShip.EnableShip();
        GenerateEnemyShip(playerShip);
        // enemyShip.ResetShipForCombat();

        // enemyShip.GetMainCabin().OnDeath.RemoveAllListeners();
        currentEnemyInstance.GetMainCabin().OnDeath.AddListener(EndCombat);

        return currentEnemyInstance;
    }

    public void UnLoadEnemyShip()
    {
        if(currentEnemyInstance != null) currentEnemyInstance.DisableShip();
        if(currentEnemyAI != null) currentEnemyAI.thinking = false;
    }

    public void LoadPlayerShip()
    {

        SetupPlayerShip();
        playerShip.GetMainCabin().OnDeath.RemoveAllListeners();
        playerShip.GetMainCabin().OnDeath.AddListener(EndCombat);
    }

    public void StartCombat()
    {
        //Time.timeScale = 1f;
        combatEnded = false;

        currentEnemyAI.ActivateAgent();
        currentEnemyAI.thinking = true;

        Debug.Log("Combat Start");
        
        combatEnded = false;
    }

    public void EndCombat(ShipComponentController mainCabin)
    {
        Debug.Log("COMBAT ENDING");
        var destroyedShip = mainCabin.shipController;

        if (destroyedShip == playerShip)
        {
            playerWon = false;
            Debug.Log("Player lost");
        }
        else if (destroyedShip == currentEnemyInstance)
        {
            playerWon=true;
            Debug.Log("Player won");
        }
        currentEnemyAI.thinking = false;
        combatEnded = true;
        gameplayFlowManager.OnCombatEnd();
    }


    // Don't use here probably (use in flow -> repair state) ....
    private bool repairing = false;
    public void StartRepairs()
    {
        repairing = true;
        MouseController.Instance.EnterRepairsMode();
    }

    public void StopRepairs()
    {
        repairing = false;
        MouseController.Instance.ExitRepairsMode();
    }
    // Don't ... -------------------


    public void ComponentDestroyed(ShipComponentController component, ShipController ship)
    {
        if(ship == playerShip)
        {
            currentEnemyInstance.AddCurrency(component.destroyRevenue);
        } else
        {
            playerShip.AddCurrency(component.destroyRevenue);
        }
    }

    // ----------------------------------------------------
    public void AssignEnemy(ShipController enemy) {
        pickedShip = enemy;
        shipGiven = true;
    }

    public ShipData AssignEnemyByDifficulty(int difficulty) {

        // TEST
        Debug.Log("Is prefab data real?: "+enemyShipPrefabs.GetRandom().shipData.name);

        var enemyShipPrefab = GetEnemyFromDifficulty(difficulty);
        Debug.Log(enemyShipPrefab);
        pickedShip = enemyShipPrefab;
        shipGiven = true;
        return enemyShipPrefab.shipData;
    }

    /// <summary>
    /// Gets enemy from given difficulty. Has a small chance to select a bit easier or harder enemy.
    /// If no enemies from +-1 range of difficulty exists, chooses first easier one.
    /// </summary>
    private ShipController GetEnemyFromDifficulty(int difficulty) {
        Debug.Log("picking from dif");
        if (enemyShipPrefabs.Count == 0) return null;


        var easierEnemies = enemyShipPrefabs.FindAll(x => x.shipData.enemyDifficulty == difficulty - 1);
        var harderEnemies = enemyShipPrefabs.FindAll(x => x.shipData.enemyDifficulty == difficulty + 1);
        var normalEnemies = enemyShipPrefabs.FindAll(x => x.shipData.enemyDifficulty == difficulty);

        // Use normal enemies list more times, so it has higher chance to be picked
        var enemyLists = new List<List<ShipController>>() {
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
        Debug.Log("No enemy found for set difficulty, picking first easier.");
        int lowerDifficulty = -1;
        for (int i = 0; i < enemyShipPrefabs.Count; i++) {
            Debug.Log("difficulty is " + enemyShipPrefabs[i].shipData.enemyDifficulty);
            if (enemyShipPrefabs[i].shipData.enemyDifficulty < difficulty) {
                lowerDifficulty = Mathf.Max(lowerDifficulty, enemyShipPrefabs[i].shipData.enemyDifficulty);
            }
        }
        Debug.Log("picked diffuclty " + lowerDifficulty);
        return enemyShipPrefabs.FindAll(x => x.shipData.enemyDifficulty == lowerDifficulty).GetRandom();
    }

    /*
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;

        string pauseText = isPaused ? "RESUME" : "PAUSE";

        if (GUI.Button(new Rect(200, 10, 150, 40), pauseText, style))
        {
            if (isPaused)
                ResumeGame();
            else
                StopGame();
        }

        if (combatEnded)
        {
            if (GUI.Button(new Rect(500, 400, 150, 40), "RESTART", style))
            {
                StartCombat();
            }

            if (!repairing)
            {
                if (GUI.Button(new Rect(500, 350, 150, 40), "BEGIN REPAIR", style))
                {
                    StartRepairs();
                }
            } else
            {
                if (GUI.Button(new Rect(500, 350, 150, 40), "STOP REPAIR", style))
                {
                    StopRepairs();
                }
            }
        }
    }*/
}
