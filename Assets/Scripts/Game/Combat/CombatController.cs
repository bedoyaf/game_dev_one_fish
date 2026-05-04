using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CombatController : SmartSingleton<CombatController>
{
    [SerializeField] private ShipController playerShip;
    [SerializeField] private ShipController enemyShip;

    [SerializeField] private List<ShipData> enemyShipDesigns;

    [SerializeField] private GameplayFlowManager gameplayFlowManager;


    public bool isPaused { private set; get; } = false;
    public bool combatEnded { private set; get; } = false;

    private EnemyShipAgent enemyShipAgent;

    public bool playerWon { get; private set; } = false;

    public void Start()
    {
        enemyShipAgent = enemyShip.GetComponent<EnemyShipAgent>();
        if(enemyShipAgent == null )
        {
            Debug.LogError("Enemy has no agent, lobotom");
        }
       // StartCombat();
    }

    public void GenerateEnemyShip()
    {
        enemyShip.shipData = enemyShipDesigns[Random.RandomRange(0, enemyShipDesigns.Count)];
        enemyShip.BuildShip();
        enemyShip.EnableShip();
        enemyShip.ResetShipForCombat();
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

    public void LoadEnemyShip()
    {
        enemyShip.EnableShip();
        GenerateEnemyShip();
        enemyShip.ResetShipForCombat();

        enemyShip.GetMainCabin().OnDeath.RemoveAllListeners();
        enemyShip.GetMainCabin().OnDeath.AddListener(EndCombat);
    }

    public void UnLoadEnemyShip()
    {
        enemyShip.DisableShip();
        if(enemyShipAgent ==null)enemyShipAgent = enemyShip.GetComponent<EnemyShipAgent>();
        enemyShipAgent.thinking = false;
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

        enemyShipAgent.ActivateAgent();

        Debug.Log("Combat ¨Start");
        
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
        else if (destroyedShip == enemyShip)
        {
            playerWon=true;
            Debug.Log("Player won");
        }
        enemyShipAgent.thinking = false;
        combatEnded = true;
        gameplayFlowManager.OnCombatEnd();
    }


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

    public void ComponentDestroyed(ShipComponentController component, ShipController ship)
    {
        if(ship == playerShip)
        {
            enemyShip.AddCurrency(component.destroyRevenue);
        } else
        {
            playerShip.AddCurrency(component.destroyRevenue);
        }
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
