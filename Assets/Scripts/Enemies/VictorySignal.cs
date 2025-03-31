using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class VictorySignal : DelayableMonoBehaviour
{
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
      // Victory menu
      MenuController.SetMenu(3);

      // Victory music
      MusicManager.PlayTrack("win", 1f);

      // Disable stuff
      Disabler.DisableAll();

      // Receipt menu
      ScoreTracker.Print(true);
    }
  }
}
