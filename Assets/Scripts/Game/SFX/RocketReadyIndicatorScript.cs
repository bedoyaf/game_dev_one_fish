using Unity.VisualScripting;
using UnityEngine;

public class RocketReadyIndicatorScript : MonoBehaviour
{
    
    [SerializeField]
    private MissileComponentController missiles;

    [SerializeField]
    private MeshRenderer quadIndicator;

    [SerializeField]
    private SpriteRenderer spriteIndicator;

    // Update is called once per frame
    void Update()
    {
        if (missiles == null)
            return;

        quadIndicator.transform.localScale = missiles.CanClick ? Vector3.one : Vector3.zero;
        spriteIndicator.transform.localScale = missiles.CanClick ? Vector3.one : Vector3.zero;
    }
}
