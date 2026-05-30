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
        yield return new WaitForSeconds(delayOnStart);

        float duration = (transform.position - targetPosition).magnitude / speed;
        transform.DOMove(targetPosition, (transform.position - targetPosition).magnitude / speed).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(duration + delayOnEnd);
        gameObject.SmartDestroy();
    }
}
