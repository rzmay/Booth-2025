using UnityEngine;

public class PlasmaCannonBullet : Bullet
{
    public float minSize = 0.25f;
    public float sizeRatio = 0.5f;
    public float damageRatio = 10f;
    public float volumeRatio = 2f;

    [SerializeField]
    private PlasmaCannonBulletShockwave _shockwavePrefab;

    [SerializeField]
    private AudioClip _collisionAudio;

    private DetachParticleSystems _detachParticleSystems;

    void Start()
    {
        Debug.Log("[PlasmaCannonBullet] start");
        _detachParticleSystems = GetComponent<DetachParticleSystems>();

        // Size should scale with damage
        transform.localScale = Vector3.one * Mathf.Max(minSize, damage * sizeRatio);
        Debug.Log($"[PlasmaCannonBullet] size set {Mathf.Max(minSize, damage * sizeRatio)}");
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
        GameObject shockwaveObject = Instantiate(_shockwavePrefab.gameObject, transform.position, transform.rotation);
        PlasmaCannonBulletShockwave shockwave = shockwaveObject.GetComponent<PlasmaCannonBulletShockwave>();
        shockwave.damage = damage * damageRatio;
        shockwave.transform.localScale = transform.localScale;
        Debug.Log($"[Shockwave] Spawning shockwave with scale {transform.localScale.x}");

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, transform.position, 1f + damage * volumeRatio);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }
}
