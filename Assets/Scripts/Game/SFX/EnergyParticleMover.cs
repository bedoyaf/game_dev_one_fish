using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnergyParticleMover : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float delayOnStart;
    [SerializeField] private float delayOnEnd;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;

    public void MoveParticles(Vector3 targetPosition) {
        startPosition = transform.position;
        endPosition = targetPosition;
        StartCoroutine(Move(targetPosition));
    }
    private IEnumerator Move(Vector3 targetPosition) {
        // Flip based on direction - TODOH DOMove with MyTime
        if (TryGetComponent<ParticleSystem>(out var psr)) {
            var direction = (targetPosition - transform.position).normalized;
            var angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            //Debug.Log($"{direction} {angle}");
            transform.localEulerAngles = transform.localEulerAngles.SetY(-angle);
            //var main = psr.main;
            //main.rotation = angle;
            //var flip = psr.flip;
            //flip.x = targetPosition.x < transform.position.x ? 1 : 0;
            //psr.flip = flip;
        }

        yield return MyTime.WaitForSeconds(delayOnStart);

        float duration = (transform.position - targetPosition).magnitude / speed;
        transform.DOMove(targetPosition, (transform.position - targetPosition).magnitude / speed).SetEase(Ease.InOutCubic);

        yield return MyTime.WaitForSeconds(duration + delayOnEnd);
        gameObject.SmartDestroy();
    }
}
