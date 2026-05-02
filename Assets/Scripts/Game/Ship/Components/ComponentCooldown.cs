using UnityEngine;

[RequireComponent(typeof(ShipComponentController))]
public class ComponentCooldown : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 2f;

    private float nextReadyTime;

    public bool IsReady => MyTime.time >= nextReadyTime;

    public float Remaining => Mathf.Max(0, nextReadyTime - MyTime.time);

    public void Trigger()
    {
        nextReadyTime = MyTime.time + cooldownTime;
    }

    public void ResetCooldown()
    {
        nextReadyTime = 0;
    }

    void OnGUI()
    {
        if (Camera.main == null) return;

        /*
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        if (screenPos.z < 0) return;

        float remaining = Remaining;

        string text = IsReady
            ? "READY"
            : remaining.ToString("0.0");

        float x = screenPos.x;
        float y = Screen.height - screenPos.y;

        GUI.color = IsReady ? Color.green : Color.red;

        GUI.Label(new Rect(x, y, 60, 20), text);
        */
    }
}
