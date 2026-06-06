using DG.Tweening;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComponentDescriptionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private TextMeshProUGUI costsText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshPreview;


    [SerializeField] private CanvasGroup group;

    public void ShowDescription(ComponentDescription description)
    {
        costsText.text = ParseString(description.costs);
        nameText.text = ParseString(description.displayName);
        descriptionText.text = ParseString(description.textDescription);

        if(description.meshFilter != null) meshFilter.mesh = description.meshFilter.sharedMesh;
        if (description.meshRenderer != null) meshPreview.materials = description.meshRenderer.sharedMaterials;

        // TODO: animation ? or fade
        panel.SetActive(true);
        group.DOFade(1f, 0.2f);
        
    }

    public void HideDescription()
    {
        group.DOFade(0f, 0.2f)
            .onComplete += () => panel.SetActive(false);
    }

    // ----- 

    private Dictionary<string, string> mapping = new Dictionary<string, string> {
        { "<energy>",   "<sprite=0>" },
        { "<currency>", "<sprite=1>" },
        { "<wheel>",    "<sprite=2>" },

    };

    // take use readable string with custom icon names
    // and convert to sprite sheet (the one we are using)
    private string ParseString(string inputText)
    {
        foreach (var kvp in mapping)
        {
            inputText = inputText.Replace(kvp.Key, kvp.Value);
        }

        return inputText;
    }

}
