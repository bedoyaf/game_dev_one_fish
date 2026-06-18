using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BannerRotateScript : MonoBehaviour
{
    [SerializeField]
    private RectTransform selfTransform;

    [SerializeField]
    private float rotateAmount = 10f;

    // how much time between rotates
    public float timeWant = 0.4f;

    private float nextTime = float.NegativeInfinity;

    private void Start()
    {
        selfTransform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > nextTime)
        {
            nextTime = Time.time + timeWant;

            // rotate randomly
            selfTransform.rotation = Quaternion.Euler(selfTransform.rotation.eulerAngles.SetZ(
                (Random.value - 0.5f) * 2f * rotateAmount));
        }
    }
}
