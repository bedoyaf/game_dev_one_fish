using UnityEngine;

public class MapUILine : MonoBehaviour
{
    public RectTransform rect;

    public void SetPositions(Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;
        float dist = dir.magnitude;

        rect.sizeDelta = new Vector2(dist, 4f); // thickness

        rect.anchoredPosition = a + dir / 2f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }
}
