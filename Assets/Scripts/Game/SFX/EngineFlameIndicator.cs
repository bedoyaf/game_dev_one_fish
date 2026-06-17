using UnityEngine;

public class EngineFlameIndicator : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem flamePrefab;

    private GameObject flamesInstance;

    [SerializeField] private Vector3 posOffset;

    [SerializeField]
    private EngineComponentController controller;

    [Header("Travel flames")]
    [SerializeField]
    private ParticleSystem travelFlamePrefab;

    private ParticleSystem travelFlamesInstance;

    [SerializeField] private Vector3 travelPosOffset;

    private void Start()
    {
        // add the particles
        flamesInstance = Instantiate(flamePrefab, transform).gameObject;
        flamesInstance.transform.localPosition = posOffset;

        if (controller != null 
            && !controller.IsPlayers)
            flamesInstance.GetComponent<ParticleSystemRenderer>().flip = new Vector3(0, 1, 0);
    }

    private void Update()
    {
        if (flamesInstance == null) {
            enabled = false;
            return;
        }

        bool inactiveInMap = GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey != GameplayFlowManager.GameStates.MapSelection;

        bool inactiveInBuilder = GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey != GameplayFlowManager.GameStates.ShipModification;

        bool? engineActive = !controller?.shipComponentController?.broken;
        bool _engineActive = engineActive != null && engineActive.Value;

        bool? shipExploding = !controller?.shipComponentController?.shipController?.IsDead;
        bool _shipExploding = shipExploding != null && shipExploding.Value;

        // disable in map
        flamesInstance.SetActive(inactiveInMap && inactiveInBuilder && _engineActive && _shipExploding);
    }

    public void SpawnTravelFlames() {
        if (controller == null || controller.shipComponentController.broken) return;
        travelFlamesInstance = Instantiate(travelFlamePrefab, transform);
        travelFlamesInstance.transform.localPosition = travelPosOffset;
    }

    public void StopTravelFlames() {
        if (travelFlamesInstance == null) return;
        var main = travelFlamesInstance.main;
        main.stopAction = ParticleSystemStopAction.Destroy;
        travelFlamesInstance.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }
}
