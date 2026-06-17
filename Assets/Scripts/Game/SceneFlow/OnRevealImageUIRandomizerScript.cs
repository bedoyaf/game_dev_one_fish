using UnityEngine;
using UnityEngine.UI;

public class OnRevealImageUIRandomizerScript : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private Image selfImage;

    private void OnEnable()
    {
        selfImage.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
