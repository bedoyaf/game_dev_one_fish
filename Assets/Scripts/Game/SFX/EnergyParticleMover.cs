using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnergyParticleMover : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float delayOnStart;
    [SerializeField] private float delayOnEnd;

    public void MoveParticles(Vector3 targetPosition) {
        StartCoroutine(Move(targetPosition));
    }
    private IEnumerator Move(Vector3 targetPosition) {
        yield return new WaitForSeconds(delayOnStart);
        targetPosition.y = transform.position.z;
        transform.DOMove(targetPosition, (transform.position - targetPosition).magnitude / speed).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(delayOnEnd);
        gameObject.SmartDestroy();
    }
}
