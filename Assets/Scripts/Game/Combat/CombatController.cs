using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Collections;

public class CombatController : SmartSingleton<CombatController>
{
    [SerializeField] private ShipController playerShip;
    // [SerializeField] private ShipController enemyShip;
    private ShipController currentEnemyInstance = null;

    [SerializeField] private Transform enemySpawnPosition;

    // [SerializeField] private List<ShipData> enemyShipDesigns;

    [SerializeField] private List<ShipController> enemyShipPrefabs;

    [SerializeField] private GameplayFlowManager gameplayFlowManager;


    [SerializeField] private Transform lootInventoryParent;

    [SerializeField] public ShipController boss;
    [SerializeField] public ShipController tutorialShip;
    [SerializeField] public TutorialController tutorialController;

    [SerializeField] public ComponentGeneratorSO componentGeneratorSO;


    [SerializeField] public ShipData tutorialPlayerShip;
    [SerializeField] public ShipData EmptyPlayerShip;

    [SerializeField] private GameUIScript gameUI;

    [SerializeField] private GameObject RemoveLootButton;
    public void InformEnemyOfComponentRemoved()
    {
        currentEnemyAI.ComponentRemoved();
    }

    public void AddComponentLoot(
        ShipComponentController lootedComponent, bool isCaptured = true)
    {

        // if has space in inventory, add as a reward
        var inventoryController = GameManager.Instance.currentGameplayManager.rewardController;
        if(inventoryController.CurrentlyHolding < inventoryController.inventoryCapacity || !isCaptured)
        {
            inventoryController.AppendComponent(lootedComponent);

            // Only show the meshes of actually stolen components
            // From my understanding, when called from tutorial end, there is no need to show the components now,
            // but only when we want to use them for the building
            if (isCaptured) {
                // separate the mesh for visuals only
                var mesh = lootedComponent.ComponentMesh;
                mesh.transform.DestroyAllChildren();
                mesh.transform.SetParent(lootInventoryParent);

                var hoverHandler = mesh.gameObject.GetComponent<LootHoverHandler>();
                if (hoverHandler == null) hoverHandler = mesh.gameObject.AddComponent<LootHoverHandler>();

                hoverHandler.Setup(RemoveLootButton, gameUI.transform);
                hoverHandler.OnRemove.AddListener(RemoveComponentFromLoot);

                // TODO: maybe destroy decor here if decide to not want it
                // include Decor child
                var decor = lootedComponent.gameObject.transform.Find("Decor");
                if (decor != null)
                    decor.gameObject.transform.SetParent(mesh.transform);

                lootedComponent.gameObject.SmartDestroy();

                // offset from each other
                mesh.transform.DOLocalMove(2f * (inventoryController.CurrentlyHolding - 1) *
                    Vector3.right, 0.5f);
            }

        }
        // when inventory full -> toss away
        else
        {
            // move off screen
            lootedComponent.transform.DOMove(
                lootedComponent.transform.position - Vector3.forward * 10, 0.5f).onComplete += 
                () => lootedComponent.gameObject.SmartDestroy();
        }

        // notify the ai that this component is
        if (currentEnemyAI != null)
            currentEnemyAI.ComponentRemoved();

    }

    public void RemoveComponentFromLoot(LootHoverHandler visualItem)
    {
        var inventoryController = GameManager.Instance.currentGameplayManager.rewardController;

        int indexToRemove = visualItem.transform.GetSiblingIndex();

        if (indexToRemove >= 0 && indexToRemove < inventoryController.storedComponents.Count)
        {
            inventoryController.storedComponents.RemoveAt(indexToRemove);
        }

        Destroy(visualItem.gameObject);

        StartCoroutine(UpdateLootPositionsDeferred());
    }

    private IEnumerator UpdateLootPositionsDeferred()
    {
        yield return new WaitForEndOfFrame();
        UpdateLootPositions();
    }

    public void UpdateLootPositions()
    {
        int index = 0;
        foreach (Transform child in lootInventoryParent)
        {
            child.DOLocalMove(2f * index * Vector3.right, 0.4f).SetEase(Ease.OutQuad);
            index++;
        }
    }



    public void ClearInventory()
    {
        // TODO: maybe some effect ?
        Debug.Log("Clearing inventory"); ;

        lootInventoryParent.DestroyAllChildren();
    }

    public bool isPaused { private set; get; } = false;
    public bool combatEnded { private set; get; } = false;

    // private EnemyShipAgent enemyShipAgent;
    private EnemyShipAgent currentEnemyAI;

    public bool playerWon { get; private set; } = false;
    public bool bossKilled { get; private set; } = false;

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
        currentEnemyInstance.AssignShipController();

        CorrectShipSprites(currentEnemyInstance);

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
        if (currentEnemyInstance.boss) {
            var mainCabins = currentEnemyInstance.GetMainCabins();
            foreach(var mcab in mainCabins) {
                mcab.OnDeath.AddListener(EvaluateBossDeath);
            }
        }
        else {
            currentEnemyInstance.GetMainCabin().OnDeath.AddListener(EndCombat);
        }

        if (gameplayFlowManager.tutorialRunning) {
            AudioManager.Instance.ToggleSFX(false);
            currentEnemyInstance.DestroyEnemyComponents(tutorialController.typesToDestroyForTutorial);
            playerShip.UseCurrency(playerShip.storedMoney);
            AudioManager.Instance.ToggleSFX(true);
        }

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
        ResetListeners();
    }

    private void ResetListeners() {
        playerShip.GetMainCabin().OnDeath.RemoveAllListeners();
        playerShip.GetMainCabin().OnDeath.AddListener(EndCombat);

        playerShip.OnSoftLock.RemoveAllListeners();
        playerShip.OnSoftLock.AddListener(OnSoftLock);
    }

    public void OnSoftLock()
    {
        gameUI.ShowResignButton();
    }

    public void StartCombat()
    {
        ResetListeners();

        if (gameplayFlowManager.tutorialRunning) {
            AudioManager.Instance.ToggleSFX(false);
            currentEnemyInstance.DestroyEnemyComponents(tutorialController.typesToDestroyForTutorial);
            playerShip.UseCurrency(playerShip.storedMoney);
            AudioManager.Instance.ToggleSFX(true);
        }

        playerShip.CheckFailState();
        //Time.timeScale = 1f;
        combatEnded = false;

        currentEnemyAI.ActivateAgent();
        currentEnemyAI.thinking = true;

        Debug.Log("Combat Start");
        
        combatEnded = false;
    }

    public void EvaluateBossDeath(ShipComponentController mainCabin) {
        var mainCabins = currentEnemyInstance.GetMainCabins();
        //bool allDead = true;
        mainCabins.RemoveAll(x => x.broken);
        //foreach (var mcab in mainCabins) {
        //    if (!mcab.broken) {
        //        allDead = false;
        //        break;
        //    }
        //}

        // On death is called before the component is broken
        if (mainCabins.Count == 1) {
            EndCombat(mainCabin);
        }
    }

    public void EndCombat(ShipComponentController mainCabin)
    {
        Debug.Log("COMBAT ENDING");
        var destroyedShip = mainCabin.shipController;

        gameUI.HideResignButton();

        // FX of explosion
        GameManager.Instance.SFXManager.ExplodeShip(destroyedShip);

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

        if (destroyedShip.boss == true) bossKilled = true;

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

    public bool debug_override = false;

    /// <summary>
    /// Gets enemy from given difficulty. Has a small chance to select a bit easier or harder enemy.
    /// If no enemies from +-1 range of difficulty exists, chooses first easier one.
    /// </summary>
    private ShipController GetEnemyFromDifficulty(int difficulty) {
        Debug.Log("picking from dif");
        if (enemyShipPrefabs.Count == 0) return null;

        if (debug_override)
            return enemyShipPrefabs.GetRandom();

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

    // Corrects sprites for shields
    private void CorrectShipSprites(ShipController ship) {
        foreach (Transform comp in ship.componentsParent.transform) {
            // Get the child named "Decor"
            var decors = comp.Find("Decor");
            if (decors != null) {
                foreach (Transform child in decors.transform) {
                    var spriteRenderer = child.GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder -= 1;
                }
            }
        }

        foreach(Transform child in ship.transform) {
            if (child == ship.componentsParent) continue;

            foreach (var sprite in child.GetComponentsInChildren<SpriteRenderer>()) {
                sprite.sortingOrder -= 10;
            }
        }

        //var wire = ship.transform.Find("wire");
        //if (wire != null) {
        //    wire.GetComponent<SpriteRenderer>().sortingOrder = -10;
        //}
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
