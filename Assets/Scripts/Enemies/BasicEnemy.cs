using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Damageable))]
public class EnemyController : MonoBehaviour
{
  public float attackRange;
  public float attackRate;
  public float attackDamage;
  public GameObject attackBone;
  public GameObject headBone;
  public Bullet projectile;
  public float projectileSpeed;

  [Range(0, 1)]
  public float projectileSpread = 0.1f;

  private Player _player;

  private Damageable _damageable;
  private NavMeshAgent _navAgent;

  private float _targetDistance;

  private float _attackCooldown;

  private Animator _animator;


  void Start()
  {
    _player = FindFirstObjectByType<Player>();

    _damageable = GetComponent<Damageable>();
    _navAgent = GetComponent<NavMeshAgent>();
    _animator = GetComponent<Animator>();

    Debug.Log($"Player: {_player}, Damageable: {_damageable}, NavAgent: {_navAgent}, Animator: {_animator}");

    _targetDistance = _navAgent.stoppingDistance;

    _damageable.onDamage += OnDamage;
  }

  void Update()
  {
    _attackCooldown -= Time.deltaTime;

    if (_animator != null) _animator.SetFloat("Speed", _navAgent.velocity.magnitude);

    // If the player isn't in sight, we need to get closer
    if (!HasLineOfSight())
    {
      Debug.Log($"[{name}] No line of sight, getting closer");
      _navAgent.stoppingDistance = 0f;
    }
    else
    {
      _navAgent.stoppingDistance = _targetDistance;
    }

    // Only attack if within close to stopping range, attack has cooled down, and line of sight
    if (_attackCooldown <= 0f && HasLineOfSight() && _navAgent.remainingDistance <= attackRange)
    {
      Debug.Log($"[{name}] Within attacking range, executing attack");
      Attack();
    }
    else if (HasLineOfSight())
    {
      Debug.Log($"[{name}] Cannot attack. Remaining distacne: {_navAgent.remainingDistance}, attackRange: {attackRange}, attackCooldown: {_attackCooldown}");
    }

    _navAgent.SetDestination(GetDestination());
  }

  bool HasLineOfSight()
  {
    return !NavMesh.Raycast(headBone.transform.position, GetDestination(), out _, NavMesh.AllAreas);
  }

  // Allow override for more complex enemy behavior
  protected Vector3 GetDestination()
  {
    return _player.transform.position;
  }

  void Attack()
  {
    _attackCooldown = attackRate;

    if (projectile != null) ProjectileAttack();
    else MeleeAttack();
  }

  protected void MeleeAttack()
  {
    Debug.Log("[Enemy] Executing melee attack");

    if (_animator != null) _animator.SetTrigger("Attack");
  }

  protected void ProjectileAttack()
  {
    Debug.Log("[Enemy] Executing projectile attack");

    // Calculate bullet direction
    Vector3 bulletSpawn = attackBone.transform.position;
    Quaternion bulletRotation = Quaternion.Slerp(
      Quaternion.FromToRotation(Vector3.forward, _player.transform.position - bulletSpawn),
      Random.rotation,
      projectileSpread
    );

    // Instantiate the bullet
    GameObject bulletObject = Instantiate(projectile.gameObject, bulletSpawn, bulletRotation);
    Bullet bullet = bulletObject.GetComponent<Bullet>();

    bullet.damage = attackDamage;
    bullet.speed = projectileSpeed;
  }

  void OnDamage(float health, float damage)
  {
    Debug.Log($"[BasicEnemy] Damage received, damage: {damage}, health: {health}");

    if (health <= 0)
    {
      if (_animator != null) _animator.SetTrigger("Death");
      else Destroy(gameObject);
    }
    else
    {
      _animator?.SetTrigger("Hit");
    }
  }

  public void OnDeathEnd()
  {
    Destroy(gameObject);
  }

  public void OnAttackHit()
  {
    // Find the player
    Collider hit = Physics.OverlapSphere(attackBone.transform.position, 0.1f)
      .First(c => c.GetComponent<Player>() != null);

    if (!hit) return;

    // Deal damage
    hit.GetComponent<Damageable>().health -= attackDamage;
  }
}
