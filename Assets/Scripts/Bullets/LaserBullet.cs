using UnityEngine;

[RequireComponent(typeof(DetachParticleSystems))] // Required even if unused to avoid null checks
[RequireComponent(typeof(LaserBulletRenderer))]
public class LaserBullet : Bullet
{
    public float damagePerSecond = 25f;
    public float maxWidth = 0.25f;

    public float speedMultiplier = 100f; // Used for force calculation

    [SerializeField]
    public LayerMask _ignoreLayers;

    [SerializeField]
    private ParticleSystem _collisionParticleSystem;

    [SerializeField]
    private AudioSource _collisionAudioSource;

    private DetachParticleSystems _detachParticleSystems;

    private LaserBulletRenderer _laserRenderer;

    private float _timeRemaining;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();
        _laserRenderer = GetComponent<LaserBulletRenderer>();

        // Damage is constant -- use damage here for time the laser fires
        _timeRemaining = damage;

        speed = speedMultiplier;
    }

    void Update()
    {
        // Check time
        if (_timeRemaining <= 0)
        {
            _detachParticleSystems.Detach();
            Destroy(gameObject);

            return;
        }

        // Decrease time
        _timeRemaining -= Time.deltaTime;

        // Track shots fired
        ScoreTracker.TrackShotsFired();

        // Raycast to find hit
        RaycastHit hit;
        float length = 0f;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 1000f, ~_ignoreLayers))
        {
            length = 100; // Long as distance out of rendering range
            _collisionAudioSource.Stop(); // Stop playing the collision audio
            _collisionParticleSystem.Stop(); // Stop playing the collision audio
        }
        else
        {
            length = (hit.point - transform.position).magnitude;

            // Move the collision audio source & particle system and play
            _collisionAudioSource.gameObject.transform.position = hit.point;
            _collisionParticleSystem.gameObject.transform.position = hit.point;

            if (!_collisionAudioSource.isPlaying) _collisionAudioSource.Play();
            if (!_collisionParticleSystem.isPlaying) _collisionParticleSystem.Play();

            // Apply damage
            float damageRatio = (damagePerSecond * Time.deltaTime) / damage; // Divide by damage to make constant
            ApplyDamage(hit.collider, hit.normal, damageRatio);
        }

        // Send to laser renderer
        _laserRenderer.length = length;
        _laserRenderer.width = maxWidth * (1f - Mathf.Pow(1f - Mathf.Clamp(_timeRemaining, 0f, 1f), 3));
    }
}
