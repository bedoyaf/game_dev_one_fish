using DG.Tweening;
using UnityEngine;

public class StarFadeScript : MonoBehaviour
{


    private SpriteRenderer selfRenderer;
    private float alpha_start = 1f;
    
    private void Start()
    {
        selfRenderer = gameObject.GetComponent<SpriteRenderer>();
        alpha_start = selfRenderer.color.a;

        // hide by default 
        selfRenderer.DOFade(0f, 0.1f);
    }

    public void Fade(float alpha, float time)
    {
        selfRenderer.DOFade(alpha * alpha_start, time)
            .SetDelay(Random.value * 0.5f);
    }

}
