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

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Spawn the particle system
        Instantiate(_collisionParticleSystem.gameObject, transform.position, transform.rotation);

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, transform.position);

        // Apply damage and physics
        Vector3 normal = collision.contacts.Length == 0 ? -transform.forward : collision.contacts[0].normal;
        ApplyDamage(collision.collider, normal);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Spawn the particle system
        Instantiate(_collisionParticleSystem.gameObject, transform.position, transform.rotation);

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, transform.position);

        // Apply damage and physics
        Vector3 normal = -transform.forward;  // Since there are no collision contacts, use the forward direction as fallback
        ApplyDamage(other, normal);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }
}
