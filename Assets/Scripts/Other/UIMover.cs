using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{
    public RectTransform targetImage;
    private float y;
    [SerializeField]
    private float offsetDistance = 550;

    [SerializeField] private float moveDuration = 0.2f;

    private void Start() {
        y = targetImage.anchoredPosition.y;
    }


    public bool visible = true;

    // Move the overlay up / down to reveal the background
    public void ToggleVisible() {
        if (visible) {
            MoveDown();
        }
        else {
            MoveUp();
        }

        visible = !visible;
    }

    private void MoveDown() {
        targetImage.DOAnchorPosY(y - offsetDistance, moveDuration);
    }

    private void MoveUp() {
        targetImage.DOAnchorPosY(y, moveDuration);
    }
}
