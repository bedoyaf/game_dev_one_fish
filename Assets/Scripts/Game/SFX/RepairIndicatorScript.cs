using UnityEngine;

public class RepairIndicatorScript : MonoBehaviour
{
    [SerializeField]
    private RepairerComponentController repairer;

    [SerializeField]
    private MeshRenderer quadIndicator;

    // Update is called once per frame
    void Update()
    {
        quadIndicator.transform.localScale = repairer.CanClickOnNow ? Vector3.one : Vector3.zero;
    }
}
