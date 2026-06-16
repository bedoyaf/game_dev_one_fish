using UnityEngine;

public class EngineFlameIndicator : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem flamePrefab;

    private GameObject flamesInstance;

    [SerializeField] private Vector3 posOffset;

    [SerializeField]
    private EngineComponentController controller;

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
        bool inactiveInMap = GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey != GameplayFlowManager.GameStates.MapSelection;

        bool inactiveInBuilder = GameManager.Instance.currentGameplayManager.stateMachine.CurrentStateKey != GameplayFlowManager.GameStates.ShipModification;

        // disable in map
        flamesInstance.SetActive(inactiveInMap && inactiveInBuilder);
        
    }
}
