using UnityEngine;


/// <summary>
/// Special Effects for the Gameplay Scene.
/// 
/// Only effects here (no transitions, scene changes etc.)
/// </summary>
public class SFXGameplayManager : SmartSingleton<SFXGameplayManager>
{
    [Tooltip("The player's ship")]
    [SerializeField]
    private GameObject playersShip;

    [Tooltip("The enemies's ship")]
    [SerializeField]
    private GameObject enemyShip;

    public void EnterPlayerShip()
    {
        // TODO: tween movement from the top maybe

    }

    public void EnterEnemyShip()
    {
        // move out of frame first

        // then animate down

        enemyShip.transform.position = 
            new Vector3(enemyShip.transform.position.x, enemyShip.transform.position.y, playersShip.transform.position.z);
    }

    public void ExitEnemyShip()
    {

        enemyShip.transform.position =
            new Vector3(enemyShip.transform.position.x, enemyShip.transform.position.y, 24f);
    }


    // ------------------------------------------------------------
    
    public Transform SFXParent;

    public GameObject missileSFXprefab;

    /// <summary>
    /// Spawns a visual representation of a rocket, that will fly from the spawn
    /// position to the target location with the given speed.
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="location"></param>
    public void SpawnRocket(Vector3 spawnPoint, Vector3 location, float time)
    {
        GameObject missile = Instantiate(
            missileSFXprefab,
            spawnPoint,
            Quaternion.LookRotation(location - spawnPoint),
            SFXParent
        );

        
        if(missile.TryGetComponent(out MissileTravelScript missileSFX))
        {
            missileSFX.startPosition = spawnPoint;
            missileSFX.endPosition = location;
            missileSFX.travelTime = time;
        }
    
    }

}
