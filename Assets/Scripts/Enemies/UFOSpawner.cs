using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Damageable))]
public class UFOSpawner : Spawner
{
  public static bool disabled = false;
  public float initialHeight = 5f;
  public float targetHeight = 2f;
  public float tractorBeamWidth = 0.5f;
  public float tractorBeamLerpFactor = 1f;

  public float inOutLerpFactor = 1f;
  public float spawnDuration = 3f;
  public float destroyDelay = 3f;
  public float spinSpeed = 360f;

  public float pitchMultiplier = 0.5f;

  [SerializeField] private GameObject _tractorBeam;
  [SerializeField] private List<AudioClip> _hitSounds;
  [SerializeField] private List<AudioClip> _destroySounds;
  [SerializeField] private ParticleSystem _destroyParticle;
  [SerializeField] private ParticleSystem _damagedParticle;

  private Damageable _damageable;
  private Rigidbody _rigidbody;
  private AudioSource _audioSource;
  private AudioSource _beamAudioSource;

  private bool _hit = false;
  private bool _spawning = false;
  private bool _spawned = false;
  private float _spawnTime = 0f;

  private Vector3 _targetPosition;

  void Start()
  {
    _damageable = GetComponent<Damageable>();
    _rigidbody = GetComponent<Rigidbody>();
    _audioSource = GetComponent<AudioSource>();
    _beamAudioSource = _tractorBeam.GetComponent<AudioSource>();

    _damageable.onDamage += OnDamage;

    // Save target position and move to height
    _targetPosition = targetPosition + (Vector3.up * targetHeight);
    transform.position = _targetPosition + (Vector3.up * initialHeight);

    // Set tractorbeam lookat
    LookAt tractorLookAt = _tractorBeam.GetComponent<LookAt>();
    tractorLookAt.target = FindFirstObjectByType<Player>()?.transform;
  }

  void OnDestroy()
  {
    _damageable.onDamage -= OnDamage;
  }

  void Update()
  {
    // Audio pitch
    _audioSource.pitch = 1 + (transform.position.y - _targetPosition.y) * pitchMultiplier;
    _audioSource.volume = Mathf.Clamp01(1 - (transform.position.y - _targetPosition.y) / initialHeight);

    // Don't do any of this if it's been hit already
    if (_hit) return;

    // Spin lol
    transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);

    if (!_spawning)
    {
      // Lerp towards target position
      transform.position = Vector3.Lerp(transform.position, _targetPosition, inOutLerpFactor * Time.deltaTime);

      if (transform.position.y - _targetPosition.y < 0.05f)
      {
        _spawning = true;
        _spawnTime = spawnDuration;
      }
    }
    else if (_spawnTime >= 0f)
    {
      _spawnTime -= Time.deltaTime;

      // Don't do any tractor beam stuff or spawning if disabled
      if (disabled) return;

      if (_spawnTime >= (spawnDuration / 2f))
      {
        // Open tractor beam
        _tractorBeam.transform.localScale = Vector3.Lerp(
          _tractorBeam.transform.localScale,
          new Vector3(tractorBeamWidth, _tractorBeam.transform.localScale.y, _tractorBeam.transform.localScale.z),
          tractorBeamLerpFactor * Time.deltaTime
        );

        // Tractor beam audio
        _beamAudioSource.volume = Mathf.Lerp(_beamAudioSource.volume, 1, tractorBeamLerpFactor * Time.deltaTime);
      }
      else
      {
        // Spawn
        if (!_spawned)
        {
          _spawned = true;
          Instantiate(prefab, targetPosition, targetRotation);

          // Remove invincibility
          if (_damageable.invincible) _damageable.invincible = false;
        }

        // Close tractor beam
        _tractorBeam.transform.localScale = Vector3.Lerp(
          _tractorBeam.transform.localScale,
          new Vector3(0, _tractorBeam.transform.localScale.y, _tractorBeam.transform.localScale.z),
          tractorBeamLerpFactor * Time.deltaTime
        );

        // Tractor beam audio
        _beamAudioSource.volume = Mathf.Lerp(_beamAudioSource.volume, 0, tractorBeamLerpFactor * Time.deltaTime);
      }
    }
    else
    {
      // Time to escape
      transform.position = Vector3.Lerp(
        transform.position,
        _targetPosition + (Vector3.up * initialHeight),
        inOutLerpFactor * Time.deltaTime
      );

      if ((_targetPosition + (Vector3.up * initialHeight)).y - transform.position.y < 0.05f)
      {
        // Goodbye world
        Destroy(gameObject);
      }
    }
  }

  void OnDamage(float health, float damage, bool isCritical)
  {
    PlaySound(_hitSounds);

    if (!_hit && health <= 0)
    {
      _hit = true;

      // Track
      ScoreTracker.TrackUFOs();

      // Hopefully this preserves the force applied, we'll have to see
      _rigidbody.isKinematic = false;
      _tractorBeam.SetActive(false);
      _damagedParticle.Play();

      Delay(() =>
      {
        // Instantiate the particle system
        Instantiate(_destroyParticle.gameObject, transform.position, Quaternion.identity);

        // Play the sound
        PlaySound(_destroySounds);

        Destroy(gameObject);
      }, destroyDelay);
    }
  }

  void OnCollisionEnter(Collision collision)
  {
    PlaySound(_hitSounds, 0.5f);
  }

  void PlaySound(List<AudioClip> clips, float volume = 1f)
  {
    AudioClip clip = clips[Random.Range(0, clips.Count)];
    AudioUtility.PlaySpatialClipAtPointWithVariation(clip, transform.position, volume);
  }
}
