using UnityEngine;
using UnityEngine.Audio;

public class ShieldActivationIndicatorScript : MonoBehaviour
{
    
    [SerializeField]
    private ShieldComponentController shield;

    [SerializeField]
    private MeshRenderer quadIndicator;

    // Update is called once per frame
    void Update()
    {
        // Show the indicator (scale) if can be clicked -> not on cooldown
        quadIndicator.transform.localScale = shield.CanClickOnNow ? Vector3.one : Vector3.zero;
    }
}
