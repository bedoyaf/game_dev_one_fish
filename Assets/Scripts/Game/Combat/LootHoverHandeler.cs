using UnityEngine;

using UnityEngine.Events;

using UnityEngine.UI;
using System;

public class LootHoverHandler : MonoBehaviour
{
    private GameObject uiButtonPrefab;
    private Transform mainCanvasTransform;

    private GameObject spawnedButton;
    public UnityEvent<LootHoverHandler> OnRemove = new UnityEvent<LootHoverHandler>();
    private Camera cam;

    public void Setup( GameObject buttonPrefab, Transform canvas)
    {
        Debug.Log("setting up removal");
        uiButtonPrefab = buttonPrefab;
        mainCanvasTransform = canvas;
        cam = Camera.main;

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    private void OnMouseEnter()
    {
        if (spawnedButton == null && uiButtonPrefab != null)
        {
            spawnedButton = Instantiate(uiButtonPrefab, mainCanvasTransform);

            var btn = spawnedButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnRemovePressed);
            }
        }
    }
    private void OnRemovePressed()
    {
        OnRemove?.Invoke(this);
    }

    private void Update()
    {
        if (spawnedButton != null && cam != null)
        {
            spawnedButton.transform.position = cam.WorldToScreenPoint(transform.position);
        }
    }

    private void OnMouseExit()
    {
        if (spawnedButton != null)
        {
            Destroy(spawnedButton);
        }
    }

    private void OnDestroy()
    {
        if (spawnedButton != null)
        {
            Destroy(spawnedButton);
        }
    }
}