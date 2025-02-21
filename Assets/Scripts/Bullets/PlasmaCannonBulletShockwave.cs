using UnityEngine;
using System.Collections.Generic;

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

    private HashSet<Damageable> _hits = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();
        _collider = GetComponent<SphereCollider>();

        _collider.radius = 0.00001f;
        _lifetime = _particleSystem.main.startLifetime.constantMax;
        _startTime = Time.time;

        Debug.Log($"[Shockwave] Spawned with lifetime {_lifetime} and scale {transform.localScale.x} at time {Time.time}");
    }

    // Update is called once per frame
    void Update()
    {
        // my wish is to blow up! I mean like get big not--
        _collider.radius = maxRadius * _time;

        // If complete, git out!!!!
        if (_time >= 1)
        {
            Debug.Log($"[Shockwave] Completed with relative time {_time} at time {Time.time}");

            _detachParticleSystems.Detach();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Damage enemy on trigger enter
        Damageable enemy = other.GetComponentInParent<Damageable>();
        if (enemy && !_hits.Contains(enemy))
        {
            Debug.Log($"[PlasmaCannonBulletShockwave] Damageable found, applying damage {damage}");
            enemy.health -= _damage;
            _hits.Add(enemy);
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
            Debug.Log($"[Shockwave] Applied force {forceRatio * _damage} to object {rb.name}");
        }
    }
}
