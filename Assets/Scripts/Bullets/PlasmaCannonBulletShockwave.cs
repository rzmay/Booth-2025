using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DetachParticleSystems))] // Required even if unused to avoid null checks
public class PlasmaCannonBulletShockwave : MonoBehaviour
{
    [System.NonSerialized]
    public float damage;
    public float maxRadius;
    public float damageFalloff = 1f;
    public float forceRatio = 1f;

    [SerializeField]
    private ParticleSystem _particleSystem;

    private SphereCollider _collider;
    private DetachParticleSystems _detachParticleSystems;

    private float _lifetime;
    private float _startTime;
    private float _time
    {
        get
        {
            return (Time.time - _startTime) / _lifetime;
        }
    }

    private float _damage
    {
        get
        {
            return damage * Mathf.Pow(1 - _time, damageFalloff);
        }
    }

    private HashSet<GameObject> _hits = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();
        _collider = GetComponent<SphereCollider>();

        _collider.radius = 0.00001f;
        _lifetime = _particleSystem.main.startLifetime.constantMax;
        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // my wish is to blow up! I mean like get big not--
        _collider.radius = maxRadius * _time;

        // If complete, git out!!!!
        if (_time >= 1)
        {
            _detachParticleSystems.Detach();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Damage enemy on trigger enter
        Damageable enemy = other.GetComponentInParent<Damageable>();

        if (enemy && !_hits.Contains(enemy.gameObject))
        {
            // Track
            ScoreTracker.TrackShotsHit();

            enemy.Damage(_damage, other);
            _hits.Add(enemy.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Add force continuously as long as collider is within shockwave
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            Vector3 direction = other.transform.position - transform.position;
            rb.AddForceAtPosition(direction.normalized * forceRatio * _damage, transform.position);
        }
    }
}
