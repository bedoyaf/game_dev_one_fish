using UnityEngine;

public class EnergyUIOrbAnimated : MonoBehaviour
{

    // Move up / down
    [SerializeField] private RectTransform orbImage;
    [SerializeField] private float moveScale = 1.0f;

    // Flip between, and sometimes none
    [SerializeField] private RectTransform linesImage1;
    [SerializeField] private RectTransform linesImage2;

    // Scale up and down
    [SerializeField] private RectTransform glowImage;


    void Update()
    {
        float t = (Time.time);
        t -= Mathf.Floor(t);
        t *= Mathf.PI * 2f;

        // up down
        orbImage.localPosition = new Vector2(0, Mathf.Sin(t) * moveScale);

        // scale up / down
        glowImage.localScale = new Vector2(1.2f + 0.1f * Mathf.Sin(t), 1.2f + 0.1f * Mathf.Sin(t));

        // swap between
        if(t < Mathf.PI * 2f / 4f)
        {
            linesImage1.gameObject.SetActive(true);
            linesImage2.gameObject.SetActive(false);

        } else if(t > 2f * Mathf.PI * 2f / 4f && t < 3f * Mathf.PI * 2f / 4f)
        {
            linesImage1.gameObject.SetActive(false);
            linesImage2.gameObject.SetActive(true);
        } else
        {
            linesImage1.gameObject.SetActive(false);
            linesImage2.gameObject.SetActive(false);
        }
    }
}
