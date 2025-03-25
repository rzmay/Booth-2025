using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{
  public float attackRange;
  public float attackRate;
  public float attackDamage;
  public Transform attackBone;
  public float attackBoneRadius = 0.1f;
  public Transform headBone;
  public Bullet projectile;
  public float projectileSpeed;

  [Range(0, 1)]
  public float projectileSpread = 0.1f;

  [SerializeField] private EnemyController _nextPhase;
  [SerializeField] private ParticleSystem _deathParticle;


  [SerializeField] private List<AudioClip> _spawnSounds;
  [SerializeField] private List<AudioClip> _attackWindupSounds;
  [SerializeField] private List<AudioClip> _attackSounds;
  [SerializeField] private List<AudioClip> _hitSounds;
  [SerializeField] private List<AudioClip> _criticalHitSounds;
  [SerializeField] private List<AudioClip> _deathSounds;

  [SerializeField] private List<AudioClip> _idleSounds;
  public float soundFrequency = 0.3f;


  private Player _player;

  private Damageable _damageable;
  private NavMeshAgent _navAgent;
  private AudioSource _audioSource;
  private Animator _animator;
  private DetachParticleSystems _detachParticleSystems;

  private LookAt[] _constraints;


  // private float _targetDistance;

  private float _attackCooldown;

  private float _soundCooldown;
  private float _gainBoost = 0f;

  void Start()
  {
    _player = FindFirstObjectByType<Player>();

    _damageable = GetComponent<Damageable>();
    _navAgent = GetComponent<NavMeshAgent>();
    _animator = GetComponent<Animator>();
    _audioSource = GetComponent<AudioSource>();
    _detachParticleSystems = GetComponent<DetachParticleSystems>();
    _constraints = GetComponentsInChildren<LookAt>();

    MetaXRAudioSource _metaAudioSource = GetComponent<MetaXRAudioSource>();
    if (_metaAudioSource) _gainBoost = _metaAudioSource.GainBoostDb;

    // _targetDistance = _navAgent.stoppingDistance;

    _damageable.onDamage += OnDamage;

    // Shouldn't attack right after spawn
    _attackCooldown = attackRate;

    PlaySound(_spawnSounds);
  }

  void OnDestroy()
  {
    _damageable.onDamage -= OnDamage;
  }

  void Update()
  {
    _attackCooldown -= Time.deltaTime;
    _soundCooldown -= Time.deltaTime;

    // Don't do any of this if dead
    if (_damageable.dead) return;

    bool hasLineOfSight = HasLineOfSight();

    if (_animator != null) _animator.SetFloat("Speed", (_navAgent.velocity.magnitude / _navAgent.speed));
    if (_player != null) SetConstraintTargets(hasLineOfSight ? _player.transform : null);

    if (_audioSource != null && _soundCooldown <= 0 && Random.value < soundFrequency * Time.deltaTime)
    {
      PlaySound(_idleSounds);
    }

    // If the player isn't in sight, we need to get closer
    // if (!HasLineOfSight())
    // {
    //   _navAgent.stoppingDistance = 0.01f;
    // }
    // else
    // {
    //   _navAgent.stoppingDistance = _targetDistance;
    // }

    // Only attack if within close to stopping range, attack has cooled down, and line of sight
    if (_attackCooldown <= 0f && HasLineOfSight() && _navAgent.remainingDistance <= attackRange)
    {
      Attack();
    }

    _navAgent.SetDestination(GetDestination());
  }

  bool HasLineOfSight()
  {
    return true;
    // Doesnt seem to be working, debug another time
    // return !NavMesh.Raycast(headBone.position, GetDestination(), out _, NavMesh.AllAreas);
  }

  private void SetConstraintTargets(Transform target)
  {
    foreach (LookAt constraint in _constraints.Where(constraint => constraint.target != target))
    {
      constraint.target = target;
    }
  }

  // Allow override for more complex enemy behavior
  protected Vector3 GetDestination()
  {
    return _player.transform.position;
  }

  void Attack()
  {
    _attackCooldown = attackRate;

    // Play sound
    PlaySound(_attackWindupSounds);

    if (_animator) _animator.SetTrigger("Attack");
    else OnAttackHit();
  }

  protected void MeleeAttack()
  {
    // Find the player
    Collider hit = Physics.OverlapSphere(attackBone.position, attackBoneRadius)
      .FirstOrDefault(c => c.GetComponent<Player>() != null);

    if (!hit) return;

    // Deal damage
    hit.GetComponent<Damageable>().Damage(attackDamage);
  }

  protected void ProjectileAttack()
  {
    // Calculate target position -- halfway between camera (head) and ground, otherwise its shooting at the face
    Vector3 targetPosition = Vector3.Scale(_player.transform.position, new Vector3(1, 0.5f, 1));

    // Calculate bullet direction
    Vector3 bulletSpawn = attackBone.position;
    Quaternion bulletRotation = Quaternion.Slerp(
      Quaternion.FromToRotation(Vector3.forward, targetPosition - bulletSpawn),
      Random.rotation,
      projectileSpread
    );

    // Instantiate the bullet
    GameObject bulletObject = Instantiate(projectile.gameObject, bulletSpawn, bulletRotation);
    Bullet bullet = bulletObject.GetComponent<Bullet>();

    bullet.damage = attackDamage;
    bullet.speed = projectileSpeed;
  }

  void OnDamage(float health, float damage, bool isCritical)
  {
    if (health <= 0)
    {
      // Death sound
      PlaySound(_deathSounds, true);

      // Animate or deathend
      if (_animator) _animator.SetTrigger("Death");
      else OnDeathEnd();
    }
    else
    {
      // Hit sounds
      PlaySound(_hitSounds);
      if (isCritical)
      {
        Debug.Log($"[EnemyController] playing critical hit sound with volume {1 + _gainBoost}");
        PlaySound(_criticalHitSounds, true);
      }

      // Animate
      _animator?.SetTrigger("Hit");
    }
  }

  public void OnDeathEnd()
  {
    // Spawn death particle system
    if (_deathParticle) Instantiate(_deathParticle.gameObject, transform.position, transform.rotation);

    // Spawn the next phase
    if (_nextPhase) Instantiate(_nextPhase.gameObject, transform.position, transform.rotation);

    // Detach particle systems
    if (_detachParticleSystems) _detachParticleSystems.Detach();

    Destroy(gameObject);
  }

  public void OnAttackHit()
  {
    if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) return;

    PlaySound(_attackSounds);

    if (projectile) ProjectileAttack();
    else MeleeAttack();
  }

  private void PlaySound(List<AudioClip> clips, bool detached = false)
  {
    if (clips.Count > 0)
    {
      AudioClip clip = clips[Random.Range(0, clips.Count)];

      if (detached)
      {
        AudioUtility.PlaySpatialClipAtPointWithVariation(clip, transform.position, 1 + _gainBoost);
      }
      else if (_audioSource)
      {
        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
        _soundCooldown = clip.length;
      }
    }
  }
}
