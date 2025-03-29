using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MetaXRAudioSource))]
public class ChargeGun : DelayableMonoBehaviour
{
    // Publicly accessible gun properties
    public float minChargeTime = 0.5f;
    public float rate = 1f;
    public float speed = 1f;
    public float maxCharge = Mathf.Infinity;
    public float chargeRatio = 0.25f;
    public float cooldownRatio = 1f;
    public float chargeDamping = 2f;
    public float pitchRatio = 1f;
    public float volumeRatio = 1f;
    public bool bulletIsChild = false;

    // Used for menu display
    public float externalDamage = 1f;

    // Calculate charge from charge time
    public float charge
    {
        get
        {
            return Mathf.Pow(_chargeTime, 1f / chargeDamping) * chargeRatio;
        }
    }

    // Image used when printing
    public Texture2D image;

    // Prefabs
    [SerializeField]
    private Bullet _bullet;

    [SerializeField]
    private AudioClip _fireSound;

    [SerializeField]
    private AudioClip _chargePitchSound;

    [SerializeField]
    private AudioClip _chargeVolumeSound;

    [SerializeField]
    private ParticleSystem _fireParticleSystem;

    // Actions
    [SerializeField]
    private InputActionReference _chargeAction;

    // Private component references
    private AudioSource _chargePitchSource;
    private AudioSource _chargeVolumeSource;

    // Private operational fields
    private bool _isCharging;
    private float _chargeTime;
    private float _timeUntilReady;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _chargeAction.action.started += OnChargeAction;
        _chargeAction.action.canceled += OnFireAction;

        AudioSource[] sources = GetComponents<AudioSource>();
        _chargePitchSource = sources[0];
        _chargeVolumeSource = sources[1];

        _chargePitchSource.clip = _chargePitchSound;
        _chargeVolumeSource.clip = _chargeVolumeSound;

        _chargeVolumeSource.volume = 0;

        foreach (AudioSource source in sources)
        {
            source.loop = true;
            source.spatialize = true;
        }
    }

    void OnDestroy()
    {
        _chargeAction.action.started -= OnChargeAction;
        _chargeAction.action.canceled -= OnFireAction;

    }

    // Update is called once per frame
    void Update()
    {
        _timeUntilReady = Mathf.Max(0, _timeUntilReady - Time.deltaTime);

        // Set menu cooldown
        MenuController.SetCooldown(_timeUntilReady / rate);

        // Charge
        if (_isCharging)
        {
            _chargeTime += Time.deltaTime;

            _chargePitchSource.pitch = 1f + (charge * pitchRatio); // Increase pitch with charge
            _chargeVolumeSource.volume = charge * volumeRatio;

            if (!_chargePitchSource.isPlaying) _chargePitchSource.Play();
            if (!_chargeVolumeSource.isPlaying) _chargeVolumeSource.Play();

            // Set menu charge
            MenuController.SetCharge(_chargeTime);
        }
    }
    private void OnChargeAction(InputAction.CallbackContext obj)
    {
        // Don't charge if not ready yet....
        if (_timeUntilReady > 0) return;

        _isCharging = true;
    }

    private void OnFireAction(InputAction.CallbackContext obj)
    {
        // Don't fire if not ready yet....
        if (_timeUntilReady > 0) return;

        // Stop charging
        _chargePitchSource.Stop();
        _chargePitchSource.time = 0;
        _chargePitchSource.pitch = 1f;

        _chargeVolumeSource.Stop();
        _chargeVolumeSource.time = 0;
        _chargeVolumeSource.volume = 0;

        if (_chargeTime >= minChargeTime)
        {
            // Track
            ScoreTracker.TrackShotsFired();

            // Set reload time
            _timeUntilReady = rate + (cooldownRatio * charge);

            // Instantiate the bullet
            GameObject bulletObject = Instantiate(_bullet.gameObject, transform.position, transform.rotation);
            if (bulletIsChild) bulletObject.transform.SetParent(transform);

            Bullet bullet = bulletObject.GetComponent<Bullet>();
            bullet.damage = charge;
            bullet.speed = speed;

            // Spawn the particle system
            Instantiate(_fireParticleSystem.gameObject, transform.position, transform.rotation);

            // Play fire sound
            AudioUtility.PlaySpatialClipAtPoint(_fireSound, transform.position);
        }

        // Reset vars
        _isCharging = false;
        _chargeTime = 0;

        // Reset menu charge
        MenuController.SetCharge(0);
    }
}
