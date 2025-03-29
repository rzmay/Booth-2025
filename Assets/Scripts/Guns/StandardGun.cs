using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MetaXRAudioSource))]
public class StandardGun : MonoBehaviour
{
    // Publicly accessible gun properties
    public float damage = 1f;
    public float rate = 1f;
    public float bulletSpeed = 1f;

    // Image used when printing
    public Texture2D image;

    // Prefabs
    [SerializeField]
    private Bullet _bullet;

    [SerializeField]
    private AudioClip _fireSound;

    [SerializeField]
    private AudioClip _reloadSound;

    [SerializeField]
    private ParticleSystem _fireParticleSystem;

    // Actions
    [SerializeField]
    private InputActionReference _fireAction;

    // Private component references
    private AudioSource _audioSource;

    // Private operational fields
    private float _timeUntilReady;
    private bool _relaodSoundPlayed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _fireAction.action.performed += OnFireAction;
    }

    void OnDestroy()
    {
        _fireAction.action.performed -= OnFireAction;
    }

    // Update is called once per frame
    void Update()
    {
        _audioSource = GetComponent<AudioSource>();

        _timeUntilReady = Mathf.Max(0, _timeUntilReady - Time.deltaTime);

        // Set menu cooldown
        MenuController.SetCooldown(_timeUntilReady / rate);

        if (_reloadSound != null && !_relaodSoundPlayed && _timeUntilReady <= _reloadSound.length)
        {
            _audioSource.PlayOneShot(_reloadSound, 1);
            _relaodSoundPlayed = true;
        }
    }

    private void OnFireAction(InputAction.CallbackContext obj)
    {
        // Don't fire if not ready yet....
        if (_timeUntilReady > 0) return;

        // Track
        ScoreTracker.TrackShotsFired();

        _timeUntilReady = rate;

        if (_reloadSound) _relaodSoundPlayed = false;

        // Instantiate the bullet
        GameObject bulletObject = Instantiate(_bullet.gameObject, transform.position, transform.rotation);
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        bullet.damage = damage;
        bullet.speed = bulletSpeed;

        // Spawn the particle system
        Instantiate(_fireParticleSystem.gameObject, transform.position, transform.rotation);

        // Play fire sound
        _audioSource.PlayOneShot(_fireSound, 1);
    }
}
