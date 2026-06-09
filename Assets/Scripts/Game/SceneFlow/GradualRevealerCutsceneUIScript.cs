using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradualRevealerCutsceneUIScript : MonoBehaviour
{

    public List<Image> images;

    public float startDelay = 1f;
    public float duration = 1f;
    public float totalDuration = 10f;

    void Start()
    {
        // start tweens
        for (int i= 0; i< images.Count; i++)
        {
            images[i].DOFade(0f, 0);

            images[i].DOFade(1f, duration)
                .SetDelay(startDelay + i * totalDuration / images.Count);
        }
    }

}
