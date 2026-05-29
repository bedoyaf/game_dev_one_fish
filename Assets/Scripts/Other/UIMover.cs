using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{
    public GameObject targetImage;
    private float y;
    [SerializeField]
    private float offsetDistance = 550;

    [SerializeField] private float moveDuration = 0.2f;

    private void Start() {
        y = targetImage.transform.position.y;
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
        targetImage.transform.DOMoveY(y - offsetDistance, moveDuration);
    }

    private void MoveUp() {
        targetImage.transform.DOMoveY(y, moveDuration);
    }
}
