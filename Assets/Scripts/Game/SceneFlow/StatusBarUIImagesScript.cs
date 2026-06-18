using UnityEngine;
using UnityEngine.UI;

public class StatusBarUIImagesScript : MonoBehaviour
{

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private Image statusImage;

    public void SetState(StatusType state)
    {
        statusImage.sprite = sprites[(int)state];
    }

}

public enum StatusType {
    None,
    Victory,
    Defeat,
    Building,
    Repair
}