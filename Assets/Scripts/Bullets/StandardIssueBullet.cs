using UnityEngine;

[RequireComponent(typeof(DetachParticleSystems))] // Required even if unused to avoid null checks
public class StandardIssueBullet : Bullet
{
    [SerializeField]
    private ParticleSystem _collisionParticleSystem;

    [SerializeField]
    private AudioClip _collisionAudio;

    private DetachParticleSystems _detachParticleSystems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();
    }

    // Update is called once per frame
    void Update()
    {
        // go my scarab
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Spawn the particle system
        Instantiate(_collisionParticleSystem.gameObject, transform.position, transform.rotation);

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, transform.position);

        // Apply damage and physics
        ApplyDamage(other.gameObject);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }
}
