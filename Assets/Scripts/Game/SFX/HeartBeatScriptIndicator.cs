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

        // Copy mesh material damage value to the sprite
        float dam = meshMat.materials[0].GetFloat("_damageAmount");
        selfMat.material.SetFloat("_damageAmount", dam);

    }
}
