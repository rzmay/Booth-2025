using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class Points : DelayableMonoBehaviour
{
  public int points = 1;

  private Damageable _damageable;
  private bool _tracked = false;

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
    if (!_tracked && health <= 0)
    {
      _tracked = true;
      ScoreTracker.Kill(points);
    }
  }
}
