using UnityEngine;

[RequireComponent(typeof(DetachParticleSystems))] // Required even if unused to avoid null checks
public class RaycastBullet : Bullet
{
    [SerializeField]
    public float speedMultiplier = 100f; // Used in calculating force -- highkey replace with boolean operation later

    [SerializeField]
    private ParticleSystem _collisionParticleSystem;

    [SerializeField]
    private AudioClip _collisionAudio;

    private DetachParticleSystems _detachParticleSystems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();
        speed = speedMultiplier;

        // We fire as soon as lil guy spawns
        Fire();
    }

    private void Fire()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit)) return;

        // Spawn the particle system
        Instantiate(_collisionParticleSystem.gameObject, hit.point, Quaternion.LookRotation(hit.normal));

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, hit.point);

        // Yuhhhh
        ApplyDamage(hit.transform.gameObject);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }
}
