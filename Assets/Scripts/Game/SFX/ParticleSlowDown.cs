using DG.Tweening;
using UnityEngine;

/// <summary>
/// Let the particles scale with current time scale
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticleSlowDown : MonoBehaviour
{
    private ParticleSystem ps;
    public bool affectChildren;
    private float currentSpeed;

    private void Start() {
        ps = GetComponent<ParticleSystem>();
        currentSpeed = 1;
        //if (main.playOnAwake) {
        //    main.playOnAwake = false;
        //    ps.Play();
        //}
    }


    void Update() {
        var timeSpeed = MyTime.CurrentTimeScale;
        if (timeSpeed != currentSpeed) {
            var main = ps.main;
            main.simulationSpeed = timeSpeed;
            currentSpeed = timeSpeed;
        }
        //ps.Simulate(
        //    MyTime.deltaTime,
        //    withChildren: affectChildren,
        //    restart: false,
        //    fixedTimeStep: false);
    }
}














// Don't mind this, don't question anything


/*
// down position + slightly random left/right
//var target = comp.transform.position.SetZ(-5f) + 10f * (0.5f - UnityEngine.Random.value) * Vector3.right;

//if (surrounding.direction == Vector3.zero || UnityEngine.Random.Range(0, 1.0f) < 0.5) {
//    var hemispheres = surrounding.hemispheres;
//    var shuffledIndexes = new List<int>() { 0, 1, 2, 3 };
//    //var target = comp.transform.position + surroundings[j].direction * 200;

//    shuffledIndexes.Shuffle();
//    for (int k = 0; k < shuffledIndexes.Count; k++) {
//        int index = shuffledIndexes[k];
//        if (hemispheres[index]) {
//            float angle = (directionAngles[index] + UnityEngine.Random.Range(-45, 45 + 1)) * Mathf.Deg2Rad;
//            var direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
//            target += direction * 200;
//            //target += new Vector3(directions[index].x, 0, directions[index].y) * 200;
//            //Debug.Log($"{index} {directions[index]} {hemispheres.ToDelimitedString()}");
//            break;
//        }
//    }
//}
//else {
//    target += surrounding.direction * 200;
//}
*/

//// Scatter them into various directions
//// tween the removed components positions
//foreach (var item in comps) {
//    // down position + slightly random left/right
//    var target = item.transform.position.SetZ(-5f)
//        + 10f * (0.5f - UnityEngine.Random.value) * Vector3.right;

//    float randomAngle = UnityEngine.Random.Range(0f, 30f);

//    // rotate
//    item.gameObject.transform.DOLocalRotate(
//        new Vector3(0f, randomAngle, 0f),
//        1f,
//        RotateMode.LocalAxisAdd
//    );

//    // move & don't destroy
//    item.gameObject.transform.DOMove(target, 2f);
//}