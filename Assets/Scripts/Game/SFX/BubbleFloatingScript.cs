using UnityEngine;

public class BubbleFloatingScript : MonoBehaviour
{

    [SerializeField]
    float velocity = 10f;

    [SerializeField]
    float maxDistance = 30f;

    Vector3 startPos;
    Vector3 direction;

    public bool moving = true;

    
    private void Start()
    {
        startPos = transform.position;

        PickRandomDirection();    
    }

    void PickRandomDirection()
    {
        // Bias to mainly up, but left right a little

        direction = 
            Vector3.forward * Random.Range(0.8f, 0.9f)
            + Vector3.right * Random.Range(-1f, 1f);
    }


    void Update()
    {


        // When offscreen -> reset position, and randomize

        if ((transform.position - startPos).magnitude > maxDistance)
        {
            if (moving) { 
                transform.position = startPos;

                PickRandomDirection();
            }
        }
        // only move when under the threshold
        else
        {
            transform.position += MyTime.deltaTime * velocity * direction;
        }
    }
}
