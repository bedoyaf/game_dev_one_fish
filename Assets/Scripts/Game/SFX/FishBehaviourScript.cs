using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public enum Moods
{
    Dead,
    Sad,
    Angry,
    VeryHappy,
    Wtf,
    ThisIsFine,
    OoO,
    JustHappy,
    Wonky,
    FeelsGood,
    Breathe,

    None

}

public class FishBehaviourScript : MonoBehaviour
{

    // Faces with moods struct
    [SerializeField]
    private Sprite[] moodSprites = new Sprite[System.Enum.GetValues(typeof(Moods)).Length];

    // Current mood
    [SerializeField]
    private Moods currentMood;

    // Mood override
    private Moods moodOverride = Moods.None;

    // Timer for the override
    private float startOverrideTime = 1f;
    private float nextTime = 1f;

    private float t_override => Mathf.Max(0f, Mathf.Min(1f, (MyTime.time - startOverrideTime) / (nextTime - startOverrideTime)));

    public void SetCurrentMood(Moods mood)
    {
        // Dead is final...
        if(currentMood != Moods.Dead)
            currentMood = mood;
    }

    public void SetMoodOverride(Moods mood, float time)
    {
        moodOverride = mood;
        startOverrideTime = MyTime.time;
        nextTime = MyTime.time + time;
    }

    public void Die(float animTime)
    {
        currentMood = Moods.Dead;
        SetMoodOverride(Moods.Dead, animTime);
    }


    private void Start()
    {

    }


    private void Update()
    {
        // Reset override
        if (moodOverride != Moods.None)
        {
            if (nextTime < MyTime.time)
            {
                moodOverride = Moods.None;
            }
        }

        var actualMood = moodOverride != Moods.None ? moodOverride : currentMood;
        movingEyesParent.SetActive(actualMood == movingEyesMood);

        // -------------------

        // Face
        if (moodSprites[(int)actualMood] != null)
        {
            faceSpriteRenderer.sprite = moodSprites[(int)actualMood];
        }

        // Body etc.
        switch (actualMood)
        {
            case Moods.Dead:
                DeadBody();
                break;

            default:
                RotateBody();
                MoveTailFins();
                MoveAround();
                break;
        }

        // limit the movement here
        var localPosition = bodyTransform.localPosition;

        localPosition = localPosition.SetX(Mathf.Max(minX, Mathf.Min(maxX, localPosition.x)));
        localPosition = localPosition.SetY(Mathf.Max(minY, Mathf.Min(maxY, localPosition.y)));

        bodyTransform.localPosition = localPosition;
    }

    // --------------------

    [Header("Movement settings")]
    public float tailMovementSpeed = 0.01f;
    public float finMovementSpeed = 1.2f;
    public float bodyMovementSpeed = 1.2f;

    public float rotateRangeTail = 10;
    public float rotateRangeFin = 5;
    public float rotateRangeBody = 3;

    [SerializeField] private Transform tailTransform;
    [SerializeField] private Transform fin1Transform;
    [SerializeField] private Transform fin2Transform;
    [SerializeField] private Transform bodyTransform;

    [SerializeField] private SpriteRenderer faceSpriteRenderer;


    private void RotateBody()
    {
        float t = (MyTime.time * bodyMovementSpeed).Fract();
        float rot = Mathf.Sin(t * Mathf.PI * 2f) * rotateRangeBody;

        bodyTransform.localRotation = Quaternion.Euler(0, 0, rot);
    }

    private void MoveTailFins()
    {
        float t = (MyTime.time * tailMovementSpeed).Fract();
        // t in [0; 1] -> [-rotateRange, rotateRange]
        // rot in [-1; 1] * rotateRange
        float rot = Mathf.Cos(t * Mathf.PI * 2f) * rotateRangeTail;

        tailTransform.localRotation = Quaternion.Euler(0, rot, 0);

        // ----

        // little slower for the fins
        t = (MyTime.time * finMovementSpeed).Fract();
        rot = Mathf.Cos(t * Mathf.PI * 2f) * rotateRangeFin;

        fin1Transform.localRotation = Quaternion.Euler(0, rot, rot);
        fin2Transform.localRotation = Quaternion.Euler(0, rot, rot);
    }

    [SerializeField]
    private float floatingSpeed = 1.0f;

    private void DeadBody()
    {
        // Rotate the body
        // var scale = -((t_override * 2f) - 1f);
        // bodyTransform.localScale = new Vector3(bodyTransform.localScale.x, scale, 1f);

        // Float the body up
        // bodyTransform.localPosition = bodyTransform.localPosition.AddY(floatingSpeed * MyTime.deltaTime);
    }

    [SerializeField]
    private float swimSpeed = 1.5f;
    [SerializeField]
    private float randomSwimDirTime = 5f;

    private Vector2 RandomDir = Vector2.zero;

    private float nextRandomDirTime = float.NegativeInfinity;
    private void MoveAround()
    {
        // Next random direction
        if (nextRandomDirTime < MyTime.time)
        {
            RandomDir = (Random.insideUnitCircle).normalized;

            nextRandomDirTime = MyTime.time + randomSwimDirTime;

            // If going left, switch scale
            bodyTransform.localScale = new Vector3(Mathf.Sign(RandomDir.x), bodyTransform.localScale.y, 1f);

            if (Mathf.Sign(RandomDir.x) != Mathf.Sign(bodyTransform.localScale.x)) {
                eyes.ForEach(eye => eye.OnFlip());
            }
        }

        // Move Randomly
        bodyTransform.localPosition += (Vector3) RandomDir * MyTime.deltaTime * swimSpeed;
    }

    [Header("Moving eyes")]
    [SerializeField] private Moods movingEyesMood;
    [SerializeField] private GameObject movingEyesParent;
    [SerializeField] private List<FishEyeMouseFollow> eyes;


    [Header("Movement bounds")]
    [SerializeField]
    private Vector2 leftBottom;

    [SerializeField]
    private Vector2 widthHeight;

    private float minX => leftBottom.x;
    private float maxX => leftBottom.x + widthHeight.x;
    private float minY => leftBottom.y;
    private float maxY => leftBottom.y + widthHeight.y;

    private Vector3 GetWorldLeft => new(leftBottom.x, 0, 0);
    private Vector3 GetWorldBottom => new(0, 0, leftBottom.y);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 size = new Vector3(widthHeight.x * transform.localScale.x, 0, widthHeight.y * transform.localScale.y);

        Vector3 pos = transform.position + GetWorldLeft* transform.localScale.x + GetWorldBottom * transform.localScale.y + size * 0.5f;

        Gizmos.DrawWireCube(pos, size);
    }


}
