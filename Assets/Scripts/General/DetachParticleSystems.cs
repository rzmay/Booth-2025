using System.Collections.Generic;
using UnityEngine;

public class DetachParticleSystems : MonoBehaviour
{

    [SerializeField]
    private List<ParticleSystem> _particleSystems;

    [SerializeField]

    private List<TrailRenderer> _trails;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Detach()
    {
        foreach (var particleSystem in _particleSystems)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            DetachGameObject(particleSystem.gameObject, particleSystem.main.startLifetime.constantMax);
        }

        foreach (var trail in _trails)
        {
            DetachGameObject(trail.gameObject, trail.time);
        }
    }

    void DetachGameObject(GameObject obj, float destroyAfter)
    {
        if (obj == gameObject) return;

        obj.transform.SetParent(null, true);

        Destroy(obj, destroyAfter);
    }
}
