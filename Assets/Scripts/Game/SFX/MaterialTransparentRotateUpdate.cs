using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MaterialTransparentRotateUpdate : MonoBehaviour
{

    SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.material.SetFloat("_rotate", 
            Mathf.Deg2Rad * gameObject.transform.localRotation.eulerAngles.y);

    }

}
