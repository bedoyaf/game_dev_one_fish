using UnityEngine;

public class DetachableParticles : MonoBehaviour
{

    //private Vector3 positionOffset;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    var deathListener = transform.parent.gameObject.AddComponent<DeathListener>();

    //    var realScale = transform.lossyScale;
    //    try {
    //        var sfxManager = GameManager.Instance.SFXManager;
    //        transform.parent = sfxManager.SFXParent;
    //    }
    //    catch {
    //        transform.parent = null;
    //    }

    //    transform.localScale = realScale;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void Detach() {
        var scale = transform.localScale;
        transform.SetParent(null, true);

        var ps = GetComponent<ParticleSystem>();

        if (ps != null) {
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        transform.localScale = scale;
    }
}
