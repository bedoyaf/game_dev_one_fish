using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;


/// <summary>
/// Special Effects for the Gameplay Scene.
/// 
/// Only effects here (no transitions, scene changes etc.)
/// </summary>
/// NOTE: no longer a singleton, because loses references on scene reload
public class SFXGameplayManager : MonoBehaviour
{

    [SerializeField] private SoundData victoryClip;
    [SerializeField] private SoundData defeatClip;
    [SerializeField] private SoundData shipEnterClip;
    public void EnterPlayerShip()
    {
        // TODO: tween movement from the top maybe

    }

    private ShipController enemyShip => GameManager.Instance.currentGameplayManager.EnemyShip;
    private ShipController playersShip => GameManager.Instance.currentGameplayManager.PlayerShip;

    private float MoveTime = 1f;
    public void EnterEnemyShip()
    {
        MyTime.CallAfterTime(0.5f, () => AudioManager.Instance.PlaySFX(shipEnterClip));
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


        if (missile.TryGetComponent(out ObjectFlyingScript missileSFX))
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
        AudioManager.Instance.PlaySFX(playerVictory ? victoryClip : defeatClip);
        var pos = statusBar.transform.position.y;
        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Wait for explosion to finish
        while (shipExplosionOngoing)
            yield return null;

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

    // --------------------------------------------------------------------

    public GameObject energyBallSFXprefab;
    public Transform energyIndicatorPosition;

    public EnergyParticleMover energyParticlesPrefab;

    public void EnergyGatheredEffect(Vector3 spawnPoint)
    {
        // Spawn an energy ball, that moves toward the 
        // energy indicator

        GameObject energyOrb = Instantiate(
            energyBallSFXprefab,
            spawnPoint,
            Quaternion.identity,
            SFXParent
        );

        
        if (energyOrb.TryGetComponent(out ObjectFlyingScript missileSFX))
        {
            missileSFX.startPosition = spawnPoint;
            missileSFX.endPosition = energyIndicatorPosition.position;
            missileSFX.travelTime = 0.5f;
        }

    }

    public void EnergyTransmissionEffect(ShipComponentController start, ShipComponentController end) {
        if (energyParticlesPrefab == null) return;
        var startPoint = start.transform.position;
        //bool playerShip = start.shipController.playerShip;
        //if (playerShip)
        //    startPoint += new Vector3(0.5f, 0, 0.5f);
        //else
        //    startPoint += new Vector3(-0.5f, 0, 0.5f);

        startPoint = start.GetComponentCenter();
        startPoint.y = 5;

        var endPoint = end.transform.position;
        //if (playerShip)
        //    endPoint += new Vector3(0.5f, 0, 0.5f);
        //else
        //    endPoint += new Vector3(-0.5f, 0, 0.5f);
        endPoint = end.GetComponentCenter();
        endPoint.y = 5;

        var energyParticles = Instantiate(
            energyParticlesPrefab,
            startPoint,
            Quaternion.identity,
            SFXParent
        );

        energyParticles.MoveParticles(endPoint);
    }

    // -----------------------------------------------------------------

    [SerializeField] private ParticleSystem shipExplosion;
    [SerializeField] private ParticleSystem componentExplosion;
    [SerializeField] private List<SoundData> explosionSounds;
    [SerializeField] private int minExplosionCount = 3;
    [SerializeField] private int maxExplosionCount = 5;
    [SerializeField] private Vector2 timeBetweenExplosions = new Vector2(0.1f, 0.5f);
    [SerializeField] private float particlesLifetime = 3;

    public void ExplodeShip(ShipController ship)
    {
        StartCoroutine(ExplodeShipCoroutine(ship));
    }

    private bool shipExplosionOngoing = false;

    private IEnumerator ExplodeShipCoroutine(ShipController ship) {
        // Take all components in the ship's grid
        var comps = ship.componentGrid.GetAllComponents();
        var cabin = ship.GetMainCabin();
        shipExplosionOngoing = true;
        // Play explode particles
        //var particles = Instantiate(shipExplosion,
        //    cabin.transform.position + Vector3.up * 5,
        //    Quaternion.identity);
        //Destroy(particles.gameObject, particlesLifetime);
        //StartCoroutine(PlayExplosionSounds());

        // Except cabin
        comps.Remove(cabin);

        // Explode in parts
        comps.Shuffle();
        int explosionCount = minExplosionCount;
        int perPhase = comps.Count / explosionCount;
        while (explosionCount < maxExplosionCount) {
            if (perPhase <= 2 || UnityEngine.Random.Range(0.0f, 1.0f) < 0.33)
                break;

            explosionCount++;
            perPhase = comps.Count / explosionCount;
        }

        for (int i = 0; i < explosionCount; i++) {
            int start = i * perPhase;
            int end = (i + 1) * perPhase;
            // On last, get all remaining components
            if (i == explosionCount - 1)
                end = comps.Count;

            for(int j = start; j < end; j++) {
                var comp = comps[j];
                var particles = Instantiate(componentExplosion, comp.GetComponentCenter() + Vector3.up * 5, Quaternion.identity);
                Destroy(particles.gameObject, particlesLifetime);
                comp.ChangeVisualToBroken();
            }

            AudioManager.Instance.PlaySFX(explosionSounds.GetRandom());
            yield return new WaitForSeconds(UnityEngine.Random.Range(timeBetweenExplosions.x, timeBetweenExplosions.y));
        }

        // Explode main cabin and scatter parts
        var shipParticles = Instantiate(shipExplosion, cabin.GetComponentCenter() + Vector3.up * 5, Quaternion.identity);
        Destroy(shipParticles.gameObject, particlesLifetime);
        AudioManager.Instance.PlaySFX(explosionSounds.GetRandom());

        // Scatter them into various directions
        // tween the removed components positions
        foreach (var item in comps) {
            // down position + slightly random left/right
            var target = item.transform.position.SetZ(-5f)
                + 10f * (0.5f - UnityEngine.Random.value) * Vector3.right;

            float randomAngle = UnityEngine.Random.Range(0f, 30f);

            // rotate
            item.gameObject.transform.DOLocalRotate(
                new Vector3(0f, randomAngle, 0f),
                1f,
                RotateMode.LocalAxisAdd
            );

            // move & don't destroy
            item.gameObject.transform.DOMove(target, 2f);
        }

        shipExplosionOngoing = false;
    }

    //private IEnumerator PlayExplosionSounds() {
    //    for (int i = 0; i < explosionCount; i++) {
    //        var sound = explosionSounds.GetRandom();
    //        AudioManager.Instance.PlaySFX(sound);
    //        yield return new WaitForSeconds(UnityEngine.Random.Range(timeBetweenExplosions.x, timeBetweenExplosions.y));
    //    }
    //}

}
