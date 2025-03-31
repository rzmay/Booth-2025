using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(Collider))]
public class TrainingEnemy : DelayableMonoBehaviour
{
  public float destroyDelay = 3f;

  [SerializeField] private ParticleSystem _destroyParticleSystem;
  [SerializeField] private List<AudioClip> _hitSounds = new();

  private Damageable _damageable;

  void Start()
  {
    _damageable = GetComponent<Damageable>();

    _damageable.onDamage += OnDamage;
  }

  void OnDestroy()
  {
    _damageable.onDamage -= OnDamage;
  }

  void OnDamage(float health, float damage, bool isCritical)
  {
    if (health <= 0)
    {
      // Start the main phase after getting killed
      SpawnSequencer.SetPhase(1);

      // Change the OST
      MusicManager.PlayTrack("play", 3f);

      // Change menu
      MenuController.SetMenu(1);

      // Disable barcode reading
      GunBarcodeReader.SetBarcodeReadingActive(false);

      // Destroy after delay
      Delay(() => Despawn(), destroyDelay);
    }
  }

  void Despawn()
  {
    // Spawn particle system
    Instantiate(_destroyParticleSystem.gameObject, transform.position, transform.rotation);

    // Destroy
    Destroy(gameObject);
  }

  void OnCollisionEnter(Collision collision)
  {
    float volume = collision.impulse.magnitude / Time.fixedDeltaTime;
    AudioClip clip = _hitSounds[Random.Range(0, _hitSounds.Count)];

    AudioUtility.PlaySpatialClipAtPoint(clip, transform.position, volume);
  }
}
