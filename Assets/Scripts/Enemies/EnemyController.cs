using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{
  public float attackRange;
  public float attackRate;
  public float attackDamage;
  public float hitAttackDelay;

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
  private TrailRenderer _attackBoneTrail;
  private LookAt[] _constraints;


  private float _targetDistance;
  private float _navAgentSpeed;

  private float _attackCooldown;

  private float _soundCooldown;
  private float _gainBoost = 0f;

  private bool _attacking = false;
  private bool _hitPlayer = false;

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

    _attackBoneTrail = attackBone.GetComponent<TrailRenderer>();
    if (_attackBoneTrail) _attackBoneTrail.emitting = false;

    _targetDistance = _navAgent.stoppingDistance;
    _navAgentSpeed = _navAgent.speed;

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

    // If at any point the animator is not in the attack state but _attacking is true, then
    // attack was interrupted and we must call OnAttackEnd
    // if (_animator && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && _attacking)
    // {
    //   Debug.Log($"[{name}:EnemyController] Safeguard caught attacking during state {_animator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
    //   OnAttackEnd();
    // }

    // Don't do any of this if dead
    if (_damageable.dead) return;

    // If it's been stopped by a bullet, start it again
    if (Mathf.Approximately(_navAgent.speed, 0f)) _navAgent.speed = _navAgentSpeed;

    bool hasPath = HasPath();
    bool hasLineOfSight = HasLineOfSight();

    if (_animator != null) _animator.SetFloat("Speed", _navAgent.velocity.magnitude / _navAgent.speed);
    if (_player != null) SetConstraintTargets(hasLineOfSight ? _player.transform : null);

    if (_audioSource != null && _soundCooldown <= 0 && Random.value < soundFrequency * Time.deltaTime)
    {
      PlaySound(_idleSounds);
    }

    // If there isn't a direct path to the player, we need to get closer
    if (!hasPath)
    {
      _navAgent.stoppingDistance = 0.01f;
    }
    else
    {
      _navAgent.stoppingDistance = _targetDistance;
    }

    // Only attack if within close to stopping range, attack has cooled down, and path of attack
    if (
      _attackCooldown <= 0f &&
      !_attacking &&
      hasPath &&
      Vector3.Distance(_player.transform.position, transform.position) <= attackRange
    )
    {
      Attack();
    }

    _navAgent.SetDestination(GetDestination());

    // Check melee hit if valid
    if (_attacking && !_hitPlayer) CheckMeleeHit();
  }

  bool HasPath()
  {
    return !NavMesh.Raycast(headBone.position, GetDestination(), out _, LayerMask.NameToLayer("Scene"));
  }

  bool HasLineOfSight()
  {
    return !Physics.Raycast(
      headBone.position,
      (GetDestination() - headBone.position).normalized,
      Vector3.Distance(headBone.position, GetDestination()) + 1f, // Plus one for safety
      LayerMask.GetMask("Scene")
    );
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
    // Obscene cooldown -- when the attack ends, this will be set to something reasonable. This just prevents refires until then
    // _attackCooldown = 1000f;
    // In theory something like this would be necessary, but I don't have anything long enough to warrant it

    // Play sound
    PlaySound(_attackWindupSounds);

    if (_animator) _animator.SetTrigger("Attack");
    else OnAttackHit();
  }

  protected void CheckMeleeHit()
  {
    // Find the player
    Collider hit = Physics.OverlapSphere(attackBone.position, attackBoneRadius)
      .FirstOrDefault(c => c.GetComponent<Player>() != null);

    if (!hit) return;

    // Deal damage
    hit.GetComponent<Damageable>().Damage(attackDamage);
    _hitPlayer = true;
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
    // If enemy gets hit, any active attack should end
    if (_attacking) OnAttackEnd();

    // Stop him in his tracks
    _navAgent.speed = 0;

    // Cooldown after hit
    _attackCooldown = Mathf.Max(_attackCooldown, hitAttackDelay);

    if (isCritical)
    {
      PlaySound(_criticalHitSounds, true, -(_gainBoost / 2));
    }

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

      if (_animator)
      {
        // Animate
        _animator.SetTrigger("Hit");
      }
    }
  }

  public void OnDeathEnd()
  {
    Debug.Log($"[{name}EnemyController] OnDeathEnd");

    // Spawn death particle system
    if (_deathParticle) Instantiate(_deathParticle.gameObject, transform.position, transform.rotation);

    // Spawn the next phase
    if (_nextPhase) Instantiate(_nextPhase.gameObject, transform.position, transform.rotation);

    // Detach particle systems
    if (_detachParticleSystems) _detachParticleSystems.Detach();

    Destroy(gameObject);
  }

  // Used for attacks spanning some time
  public void OnAttackStart()
  {
    _attacking = true;
    if (_attackBoneTrail) _attackBoneTrail.emitting = true;
  }

  public void OnAttackEnd()
  {
    _hitPlayer = false;
    _attacking = false;
    _attackCooldown = attackRate;

    if (_attackBoneTrail) _attackBoneTrail.emitting = false;
  }

  // Used for instantaneous attacks (such as a firing projectile)
  public void OnAttackHit()
  {
    PlaySound(_attackSounds);

    if (projectile) ProjectileAttack();
    else CheckMeleeHit();

    OnAttackEnd();
  }

  private void PlaySound(List<AudioClip> clips, bool detached = false, float gainBoost = 0f)
  {
    if (clips.Count > 0)
    {
      AudioClip clip = clips[Random.Range(0, clips.Count)];

      if (detached)
      {
        AudioUtility.PlaySpatialClipAtPointWithVariation(clip, transform.position, 1 + _gainBoost + gainBoost);
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
