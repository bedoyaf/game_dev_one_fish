using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    [SerializeField] private ShipController playerShip;
    [SerializeField] private ShipController enemyShip;

    [SerializeField] private List<ShipData> enemyShipDesigns;


    public bool isPaused { private set; get; } = false;
    public bool combatEnded { private set; get; } = false;

    private EnemyShipAgent enemyShipAgent;

    public void Start()
    {
        enemyShipAgent = enemyShip.GetComponent<EnemyShipAgent>();
        if(enemyShipAgent == null )
        {
            Debug.LogError("Enemy has no agent, lobotom");
        }
        StartCombat();
    }

    public void GenerateEnemyShip()
    {
        enemyShip.shipData = enemyShipDesigns[0];
        enemyShip.BuildShip();
        enemyShipAgent.ActivateAgent();
    }

    private void SetupPlayerShip()
    {
        enemyShip.ResetShipForCombat();
    }

    public void StopGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed");
    }

    public void StartCombat()
    {
        //Time.timeScale = 1f;
        combatEnded = false;

        Debug.Log("Combat Restarted");
        GenerateEnemyShip();
        SetupPlayerShip();
        enemyShip.ResetShipForCombat();
        combatEnded = false;

        playerShip.GetMainCabin().OnDeath.RemoveAllListeners();
        enemyShip.GetMainCabin().OnDeath.RemoveAllListeners();

        playerShip.GetMainCabin().OnDeath.AddListener(EndCombat);
        enemyShip.GetMainCabin().OnDeath.AddListener(EndCombat);
    }

    public void EndCombat(ShipComponentController mainCabin)
    {
        var destroyedShip = mainCabin.shipController;

        if (destroyedShip == playerShip)
        {
            Debug.Log("Player lost");
        }
        else if (destroyedShip == enemyShip)
        {
            Debug.Log("Player won");
        }
        enemyShipAgent.thinking = false;
        combatEnded = true;
    }

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
            if (GUI.Button(new Rect(400, 400, 150, 40), "RESTART", style))
            {
                StartCombat();
            }
        }
    }
}
