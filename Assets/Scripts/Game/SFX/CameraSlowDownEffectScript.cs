using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSlowDownEffectScript : MonoBehaviour
{

    // Watch MyTime slowdown scale
    // Zoom the camera a bit, when zooming
    // maybe could do a grayscale effect too ?


    [SerializeField]
    private float zoomAmount = 1f;

    private float startSize;
    private Camera selfCamera;

    private void Start()
    {
        selfCamera = GetComponent<Camera>();
        startSize = selfCamera.orthographicSize;
        currentTimeScale = MyTime.slowDownOverride;
    }

    private float currentTimeScale; 

    void Update()
    {
        // transition to it
        if(MyTime.slowDownOverride != currentTimeScale)
        {
            currentTimeScale = MyTime.slowDownOverride;

            selfCamera.DOOrthoSize(startSize + zoomAmount * (1.0f - currentTimeScale), 0.1f);
        }
    }
}
