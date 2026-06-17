using UnityEngine;

public class HeartBeatScriptIndicator : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float scaleAmount = 0.1f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    [SerializeField] private MeshRenderer meshMat;
    [SerializeField] private SpriteRenderer selfMat;

    void Update()
    {
        float pulse = 1f + Mathf.Sin(MyTime.time * speed) * scaleAmount;
        transform.localScale = baseScale * pulse;

        if (!meshMat.gameObject.scene.IsValid())
        {
            return; // it's a prefab asset
        }

        if (meshMat != null &&
            meshMat.materials != null &&
            meshMat.materials.Length > 0 &&
            meshMat.materials[0] != null)
        {
            float dam = meshMat.materials[0].GetFloat("_damageAmount");

            if (dam >= 1f)
            {
                scaleAmount = 0f;
            }
        }
    }
}
