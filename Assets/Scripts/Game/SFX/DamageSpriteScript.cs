using UnityEngine;

public class DamageSpriteScript : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshMat;
    [SerializeField] private SpriteRenderer selfMat;

    // Update is called once per frame
    void Update()
    {
        if (!meshMat.gameObject.scene.IsValid())
        {
            return; // it's a prefab asset
        }

        // Copy mesh material damage value to the sprite
        if (meshMat != null &&
            meshMat.materials != null &&
            meshMat.materials.Length > 0 &&
            meshMat.materials[0] != null)
        {
            float dam = meshMat.materials[0].GetFloat("_damageAmount");
            selfMat.material.SetFloat("_damageAmount", dam);

            // broke the sprite, stop moving
            if (dam >= 1f)
            {
                selfMat.color = Color.gray;
            } else
            {
                selfMat.color = Color.white;
            }
        }
    }
}
