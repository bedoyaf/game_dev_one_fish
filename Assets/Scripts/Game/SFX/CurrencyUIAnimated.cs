using UnityEngine;

public class CurrencyUIAnimated : MonoBehaviour
{
    [SerializeField] private RectTransform gear1;
    [SerializeField] private RectTransform gear2;
    [SerializeField] private RectTransform gear3;

    [SerializeField] private float speed = 1f;

    // Update is called once per frame
    void Update()
    {
        gear1.Rotate(Vector3.forward, Time.deltaTime * speed);
        gear2.Rotate(Vector3.forward, Time.deltaTime * speed);
        gear3.Rotate(Vector3.forward, Time.deltaTime * speed);
    }
}
