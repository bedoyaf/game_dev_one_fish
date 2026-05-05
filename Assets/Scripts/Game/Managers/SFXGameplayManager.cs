using DG.Tweening;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;


/// <summary>
/// Special Effects for the Gameplay Scene.
/// 
/// Only effects here (no transitions, scene changes etc.)
/// </summary>
/// NOTE: no longer a singleton, because loses references on scene reload
public class SFXGameplayManager : MonoBehaviour
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

    private float MoveTime = 1f;
    public void EnterEnemyShip()
    {
        enemyShip.transform.DOMoveZ(playersShip.transform.position.z, MoveTime);
    }

    public void ExitEnemyShip()
    {
        enemyShip.transform.DOMoveZ(24f, MoveTime);
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


        if (missile.TryGetComponent(out MissileTravelScript missileSFX))
        {
            missileSFX.startPosition = spawnPoint;
            missileSFX.endPosition = location;
            missileSFX.travelTime = time;
        }

    }


    // ---------------------------------------------------------------------

    [SerializeField]
    private TMPro.TMP_Text statusBar;

    // NOTE: maybe move constants from the methods to like here...
    public void CombatStartTransition(string enemyName, Action onFinished)
    {
        StartCoroutine(CombatStartTransitionCoroutine(enemyName, onFinished));
    }

    private IEnumerator CombatStartTransitionCoroutine(string enemyName, Action onFinished)
    {
        // Show the combat text in ui
        statusBar.text = $"--- Fight ---\n{enemyName}";
        var pos = statusBar.transform.position.y;
        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Leave the text for the player to read

        yield return new WaitForSeconds(3f);

        // Move the text up

        statusBar.transform.DOMoveY(pos + 500, 0.5f);

        yield return new WaitForSeconds(0.5f);

        statusBar.gameObject.SetActive(false);
        statusBar.DOFade(0f, 0f);
        statusBar.transform.DOMoveY(pos, 0f);

        onFinished();
    }


    public void CombatEndTransition(bool playerVictory, Action onFinished)
    {
        StartCoroutine(CombatEndTransitionCoroutine(playerVictory, onFinished));
    }

    private IEnumerator CombatEndTransitionCoroutine(bool playerVictory, Action onFinished)
    {
        // Show victory / loss
        statusBar.text = playerVictory ? "Victory" : "Defeat";
        var pos = statusBar.transform.position.y;
        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Leave the text for the player to read
        yield return new WaitForSeconds(1f);

        // Move the ship
        ExitEnemyShip();

        yield return new WaitForSeconds(2f);

        // Move the text up

        statusBar.transform.DOMoveY(pos + 500, 0.5f);

        yield return new WaitForSeconds(0.5f);

        statusBar.gameObject.SetActive(false);
        statusBar.DOFade(0f, 0f);
        statusBar.transform.DOMoveY(pos, 0f);

        onFinished();

    }
}
