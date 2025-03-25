using UnityEngine;

public class PlasmaCannonBullet : Bullet
{
    public float minSize = 0.25f;
    public float sizeRatio = 0.5f;

    public float shockwaveSizeRatio = 2f;
    public float damageRatio = 10f;
    public float volumeRatio = 2f;

    [SerializeField]
    private PlasmaCannonBulletShockwave _shockwavePrefab;

    [SerializeField]
    private AudioClip _collisionAudio;

    private DetachParticleSystems _detachParticleSystems;

    void Start()
    {
        _detachParticleSystems = GetComponent<DetachParticleSystems>();

        // Size should scale with damage
        transform.localScale = Vector3.one * Mathf.Max(minSize, damage * sizeRatio);
    }

    // Update is called once per frame
    void Update()
    {
        // go my scarab
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Spawn the particle system
        GameObject shockwaveObject = Instantiate(_shockwavePrefab.gameObject, transform.position, transform.rotation);
        PlasmaCannonBulletShockwave shockwave = shockwaveObject.GetComponent<PlasmaCannonBulletShockwave>();
        shockwave.damage = damage * damageRatio;
        shockwave.transform.localScale = (transform.localScale * shockwaveSizeRatio) / sizeRatio;

        Vector3 normal = collision.contacts.Length == 0 ? -transform.rotation.eulerAngles : collision.contacts[0].normal;

        // Spawn the decal
        // Rotate randomly around z axis
        Quaternion rotation = Quaternion.Euler(
            -normal.x,
            -normal.y,
            Random.Range(0f, 360f)
        );

        // Instantiate the decal
        GameObject decalObject = Instantiate(decal.gameObject, transform.position, rotation);
        decalObject.transform.localScale = shockwave.transform.localScale; // Copy shockwave scale

        // Play the sound
        AudioUtility.PlaySpatialClipAtPointWithVariation(_collisionAudio, transform.position, 1f + (damage * damageRatio) * volumeRatio);

        // And then die!!!!!!
        _detachParticleSystems.Detach();
        Destroy(gameObject);
    }
}
