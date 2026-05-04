using UnityEngine;

public class UIInstancerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject gameplayUIprefab;

    private Canvas instance;
    public Canvas GetUICanvas => instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add the UI
        instance = Instantiate(gameplayUIprefab, transform).GetComponent<Canvas>();
        Debug.Log($"DSADA {instance}");
    }

}
