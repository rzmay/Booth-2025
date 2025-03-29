using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MetaXRAudioSource))]
public class ContinuousGun : DelayableMonoBehaviour
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
    private ParticleSystem _fireParticleSystem;

    // Actions
    [SerializeField]
    private InputActionReference _fireAction;

    // Private component references
    private AudioSource _audioSource;

    // Private operational fields
    private bool _isFiring;
    private float _timeUntilReady;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _fireAction.action.started += OnFireAction;
        _fireAction.action.canceled += OnStopFireAction;

        _audioSource = GetComponent<AudioSource>();

        // Set up the audio source to play the charge sound
        _audioSource.loop = true;
        _audioSource.clip = _fireSound;
    }

    void OnDestroy()
    {
        _fireAction.action.started -= OnFireAction;
        _fireAction.action.canceled -= OnStopFireAction;
    }

    // Update is called once per frame
    void Update()
    {
        _timeUntilReady = Mathf.Max(0, _timeUntilReady - Time.deltaTime);

        // Charge
        if (_isFiring) Fire();
    }
    private void OnFireAction(InputAction.CallbackContext obj)
    {
        _isFiring = true;
    }

    private void OnStopFireAction(InputAction.CallbackContext obj)
    {
        _isFiring = false;

        // Stop the sound & particle
        _audioSource.Stop();
        _fireParticleSystem.Stop();
    }

    private void Fire()
    {
        // Effects
        if (!_audioSource.isPlaying) _audioSource.Play();
        if (!_fireParticleSystem.isPlaying) _fireParticleSystem.Play();

        // Fire a new bullet
        if (_timeUntilReady <= 0)
        {
            // Track
            ScoreTracker.TrackShotsFired();

            // Set reload time
            _timeUntilReady = rate;

            // Instantiate the bullet
            GameObject bulletObject = Instantiate(_bullet.gameObject, transform.position, transform.rotation);
            Bullet bullet = bulletObject.GetComponent<Bullet>();

            bullet.damage = damage;
            bullet.speed = bulletSpeed;
        }
    }
}
