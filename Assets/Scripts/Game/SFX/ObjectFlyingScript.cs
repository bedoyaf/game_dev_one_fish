using UnityEngine;

public class ObjectFlyingScript : MonoBehaviour
{
    // TODO: fancy flying animation or whatever

    public Vector3 startPosition;
    public Vector3 endPosition;
    public float travelTime;


    
    private Vector3 dir;
    private float speed;

    private float time = 0;

    private void Start()
    {
        transform.position = startPosition;

        dir = (endPosition - startPosition);
        speed = dir.magnitude / travelTime;
        dir = dir.normalized;
    }

    void Update()
    {
        transform.position += dir * speed * MyTime.deltaTime;

        time += MyTime.deltaTime;
        if (time > travelTime)
        {
            Destroy(gameObject);
        }
    }
}
