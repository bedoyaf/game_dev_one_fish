using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

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

    public void SuperSpeedEnemyShipExit() {
        enemyShip.transform.DOKill();
        enemyShip.transform.DOMoveZ(24f, 0);
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
    // hack :(
    private float status_y = 330;


    // NOTE: maybe move constants from the methods to like here...
    public void CombatStartTransition(string enemyName, Action onFinished)
    {
        Debug.Log($"Combat start w '{enemyName}'");
        this.StopAllCoroutines();
        StartCoroutine(CombatStartTransitionCoroutine(enemyName, onFinished));
    }

    private IEnumerator CombatStartTransitionCoroutine(string enemyName, Action onFinished)
    {
        statusBar.DOKill();
        statusBar.GetComponent<RectTransform>().DOKill();
        // Show the combat text in ui
        statusBar.text = $"--- Fight ---\n{enemyName}";
        // var pos = statusBar.GetComponent<RectTransform>().position.y;

        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);

        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Leave the text for the player to read

        yield return new WaitForSeconds(3f);

        // Move the text up

        statusBar.GetComponent<RectTransform>().DOAnchorPos3DY(status_y + 500, 0.5f);

        yield return new WaitForSeconds(0.5f);

        statusBar.gameObject.SetActive(false);
        statusBar.DOFade(0f, 0f);
        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);

        onFinished();
        //Debug.Log("Here5");
    }

    public void EncounterTransition(string name)
    {
        StartCoroutine(EncounterTransitionCoroutine(name));
    }

    private IEnumerator EncounterTransitionCoroutine(string name)
    {
        statusBar.DOKill();
        statusBar.GetComponent<RectTransform>().DOKill();
        // Show the combat text in ui
        statusBar.text = $"{name}";

        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);

        // var pos = statusBar.GetComponent<RectTransform>().position.y;
        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Leave the text for the player to read

        yield return new WaitForSeconds(3f);

        // Move the text up

        statusBar.GetComponent<RectTransform>().DOAnchorPos3DY(status_y + 500, 0.5f);

        yield return new WaitForSeconds(0.5f);

        statusBar.gameObject.SetActive(false);
        statusBar.DOFade(0f, 0f);
        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);
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
        // var pos = statusBar.GetComponent<RectTransform>().position.y;

        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);

        statusBar.gameObject.SetActive(true);
        statusBar.DOFade(0f, 0f);
        statusBar.DOFade(1f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        // Wait for explosion to finish
        while (shipExplosionOngoing) {
            yield return null;
            // Debug.Log("Waiting");
        }

        // Leave the text for the player to read
        yield return new WaitForSeconds(1f);

        // Move the ship
        ExitEnemyShip();

        yield return new WaitForSeconds(2f);

        // Move the text up

        statusBar.GetComponent<RectTransform>().DOAnchorPos3DY(status_y + 500, 0.5f);

        yield return new WaitForSeconds(0.5f);

        statusBar.gameObject.SetActive(false);
        statusBar.DOFade(0f, 0f);
        statusBar.GetComponent<RectTransform>().anchoredPosition =
            statusBar.GetComponent<RectTransform>().anchoredPosition.SetY(status_y);

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
        startPoint = start.GetComponentCenter();
        startPoint.y = 5;

        var endPoint = end.transform.position;

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
    [SerializeField] private SoundData[] componentExplosionSounds;
    [SerializeField] private SoundData shipExplosionSound;
    [SerializeField] private float waitAfterShipExplosionTime = 2;
    [SerializeField] private float waitBeforeShipExplosionTime = 0.5f;
    [SerializeField] private Vector2 timeBetweenExplosions = new Vector2(0.1f, 0.5f);
    [SerializeField] private float particlesLifetime = 1;

    public void ExplodeShip(ShipController ship)
    {
        StartCoroutine(ExplodeShipCoroutine(ship));
    }

    private bool shipExplosionOngoing = false;

    private IEnumerator ExplodeShipCoroutine(ShipController ship) {
        // Disable clicking
        //GameManager.Instance.currentGameplayManager.EnemyShip.DisableAllCollidersExcept(new ComponentType[] {});

        // Stop targeting
        MouseController.Instance.StopTargetingAndRefund();

        // Take all components in the ship's grid
        var comps = ship.componentGrid.GetAllComponents();
        var cabin = ship.GetMainCabin();
        var mainCabins = ship.GetMainCabins();

        if (ship.boss && ship.bossMainMainComponent != null) {
            cabin = ship.bossMainMainComponent.shipComponentController;
        }

        var shake = ship.componentsParent.transform.DOShakePosition(100f, 0.2f);
        Tweener wireShake = null; 
        var wire = ship.transform.Find("wire");
        if (wire != null) {
            wireShake = ship.componentsParent.transform.DOShakePosition(100f, 0.2f);
        }

        shipExplosionOngoing = true;
        foreach(var mc in mainCabins) {
            comps.Remove(mc);
        }

        // Explode in parts
        comps.Shuffle();

        bool canTwoAtOnce = comps.Count > 5;
        while(comps.Count > 0) {
            var surroundings = AnalyzeComponentSurroundings(comps, ship);

            // How many will we explode now
            int explodeCount = UnityEngine.Random.Range(1, canTwoAtOnce ? 3 : 2);

            // Do explosion effect on each component and blast it away
            for(int j = 0; j < Mathf.Min(explodeCount, surroundings.Count); j++) {
                var surrounding = surroundings[j];
                var comp = surrounding.component;
                var particles = Instantiate(componentExplosion, comp.GetComponentCenter() + Vector3.up * 5, Quaternion.identity);
                //Destroy(particles.gameObject, particlesLifetime);
                comp.ChangeVisualToBroken();
                ship.componentGrid.RemoveComponent(comp.placementRules.connectedTile, false, false);
                comps.Remove(comp);

                // Explosion will blast the components away from the center with some randomization
                var target = comp.GetComponentCenter();
                var direction = (target - cabin.GetComponentCenter()).normalized;

                var angle = UnityEngine.Random.Range(-30, 31) * Mathf.Deg2Rad;
                direction = new Vector3(
                    direction.x * Mathf.Cos(angle) - direction.z * Mathf.Sin(angle),
                    0,
                    direction.x * Mathf.Sin(angle) + direction.z * Mathf.Cos(angle));

                target += direction.normalized * 200;


                //rotate
                float randomAngle = UnityEngine.Random.Range(0f, 360f) * 10;
                comp.gameObject.transform.DOLocalRotate(
                    new Vector3(0f, randomAngle, 0f),
                    10f,
                    RotateMode.LocalAxisAdd
                );

                CameraShake.Instance.Shake(0.1f, 0.1f);

                // move & don't destroy
                comp.gameObject.transform.DOMove(target, 30f);
            }

            if (componentExplosionSounds != null && componentExplosionSounds.Length > 0)
            {
                SoundData randomExplosionSound = componentExplosionSounds[UnityEngine.Random.Range(0, componentExplosionSounds.Length)];
                AudioManager.Instance.PlaySFX(randomExplosionSound);
            }

            yield return MyTime.WaitForSeconds(UnityEngine.Random.Range(timeBetweenExplosions.x, timeBetweenExplosions.y));
        }

        yield return MyTime.WaitForSeconds(waitBeforeShipExplosionTime);
        CameraShake.Instance.Shake();
        // Explode main cabin and scatter parts
        foreach (var mc in mainCabins) {
            var shipParticles = Instantiate(shipExplosion, mc.GetComponentCenter() + Vector3.up * 5, Quaternion.identity);
            MyTime.CallAfterTime(particlesLifetime, () => {
                if (shipParticles != null) {
                    foreach (var particle in shipParticles.GetComponentsInChildren<ParticleSystem>()) {
                        var main = particle.main;
                        main.stopAction = ParticleSystemStopAction.Destroy;

                        particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                    }

                }
            });
            AudioManager.Instance.PlaySFX(shipExplosionSound);
            yield return MyTime.WaitForSeconds(0.1f);
        }

        //Destroy(shipParticles.gameObject, particlesLifetime);
        shake.Kill();
        if (wireShake != null) wireShake.Kill();
        yield return MyTime.WaitForSeconds(waitAfterShipExplosionTime);

        shipExplosionOngoing = false;
    }

    private static Vector2Int[] directions = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
    private static int[] directionAngles = new[] { 180, 0, 270, 90 };
    
    // Does not work on properly with big components
    // Basically looks around components and 
    private List<ComponentSurroundingsData> AnalyzeComponentSurroundings(List<ShipComponentController> comps, ShipController ship) {
        List<ComponentSurroundingsData> surroundings = new();

        var componentGrid = ship.componentGrid;
        for (int i = 0; i < comps.Count; i++) {
            var comp = comps[i];
            var tile = comp.placementRules.connectedTile;

            //var hemispheres = new List<bool>() { true, true, true, true };
            Vector2Int sum = Vector2Int.zero;
            // Figure out orientation
            int k = 0;
            int found = 0;
            foreach (var dir in directions) {
                int x = tile.x - dir.x;
                if (playersShip) x = tile.x + dir.x;
                int z = tile.z + dir.y;

                if (componentGrid.ValidCoordinates(x, z) && !componentGrid[z, x].isPlaceholder) {
                    sum += dir;
                    //hemispheres[k] = false;
                    found++;
                }
                k++;
            }

            //if (found == 4)
            //    hemispheres[UnityEngine.Random.Range(0, 4)] = true;

            int neighbors = componentGrid.GetTilesAroundComponent(tile.x, tile.z).Where(x => !x.isPlaceholder).ToList().Count;

            ComponentSurroundingsData componentSur = new ComponentSurroundingsData();
            componentSur.component = comp;
            componentSur.neighbors = neighbors;
            //componentSur.hemispheres = hemispheres;

            if (sum != Vector2Int.zero) {
                componentSur.direction = new Vector3(sum.x, 0, sum.y).normalized;
            }

            surroundings.Add(componentSur);
        }

        surroundings.Sort((x, y) => x.neighbors.CompareTo(y.neighbors));
        return surroundings;
    }

    private struct ComponentSurroundingsData {
        public ShipComponentController component;
        public int neighbors;
        public Vector3 direction;
        //public List<bool> hemispheres;
    }

    [SerializeField]
    private FishBehaviourScript fishSfx;

    /// <summary>
    /// What face to set
    /// </summary>
    /// <param name="mood"> The face </param>
    /// <param name="temporary"> if only for a little bit</param>
    /// <param name="duration"> how long that little bit is </param>
    public void SetFishFace(Moods mood, bool temporary=true, float duration=1.0f)
    {
        if(mood == Moods.Dead)
        {
            fishSfx.Die(duration);
        }
        else
        {
            if(temporary)
            {
                fishSfx.SetMoodOverride(mood, duration);
            } else
            {
                fishSfx.SetCurrentMood(mood);
            }
        }
    }

    [SerializeField]
    private Transform bubblesParent;

    [SerializeField]
    private Transform starsParent;

    [SerializeField]
    private SpriteRenderer lightBack;

    [SerializeField]
    private SpriteRenderer ground;


    public void SetDayTime(bool night, bool fast=false)
    {

        foreach (Transform t in bubblesParent)
        {
            t.GetComponent<BubbleFloatingScript>().moving = !night;
        }

        // delay 
        DOVirtual.DelayedCall(fast ? 0.1f : 1.6f, () =>
        {
            lightBack.DOKill();

            if (night)
            {
                lightBack.DOFade(0f, 1.5f);
            }
            else
            {
                lightBack.DOFade(1f, 1.5f);
            }

            // tiny delay 

            DOVirtual.DelayedCall(2f, () =>
            {
                foreach (Transform t in starsParent)
                {
                    t.GetComponent<StarFadeScript>().Fade(night ? 1f : 0f, 3f);
                }
            });
        });
    }


    [SerializeField]
    private Transform plantsParent;

    [SerializeField]
    private ParticleSystem SpeedParticlesPrefab;

    private GameObject speedInstance = null;

    private float camera_start_size = 8.93f;
    private float camera_start_x = 17.06f;
    
    private float camera_map_size = 6.42f;
    private float camera_map_x = 13.44f;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Transform playerShipShaker;

    private Tween shaking = null;

    public void Lightspeed(bool active)
    {



        // show / hide everything
        foreach (Transform t in plantsParent)
        {
            t.DOKill();
            t.GetComponentInChildren<SpriteRenderer>().DOFade(active ? 0f : 1f, 
                0.5f).SetDelay(UnityEngine.Random.value * 0.3f);
        }

        foreach (Transform t in bubblesParent)
        {
            t.GetComponent<BubbleFloatingScript>().moving = !active;
        }

        lightBack.DOKill();
        lightBack.DOFade(active ? 0f : 1f, 0.5f);
        // ground.DOKill();
        // ground.DOFade(active ? 0f : 1f, 0.5f).SetDelay(0.2f);

        mainCamera.DOKill();

        if (active)
        {
            // show particles
            speedInstance = Instantiate(SpeedParticlesPrefab).gameObject;

            // move cam
            mainCamera.DOOrthoSize(camera_map_size, 1f);
            mainCamera.transform.DOMoveX(camera_map_x, 1f);

            mainCamera.GetComponent<CameraMouseFollowScript>().MovedXTo(camera_map_x);

            // shake player
            shaking = playerShipShaker.transform.DOLocalMoveX
                (0.1f, 0.3f).SetLoops(-1, LoopType.Yoyo);

        } 
        else
        {
            // stop particles emitting
            speedInstance.GetComponent<ParticleSystem>().Stop();
            // kill particles
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (speedInstance != null)
                {
                    Destroy(speedInstance);
                    speedInstance = null;
                }

                // move cam
                mainCamera.DOOrthoSize(camera_start_size, 1f);
                mainCamera.transform.DOMoveX(camera_start_x, 1f);

                mainCamera.GetComponent<CameraMouseFollowScript>().MovedXTo(camera_start_x);
            });

            // stop shaking
            if (shaking != null)
            {
                shaking.Kill();
                shaking = null;
            }

            
        }
    }


}
