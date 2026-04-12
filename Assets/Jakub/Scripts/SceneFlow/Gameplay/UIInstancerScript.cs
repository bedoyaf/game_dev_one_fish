using UnityEngine;

public class UIInstancerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject gameplayUIprefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add the UI
        Instantiate(gameplayUIprefab, transform);
    }

}
