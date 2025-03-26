using UnityEngine;
using UnityEngine.AI;

public class Disabler
{
  public static void DisableAll()
  {
    GunManager.DisableGuns();

    // No more spawns
    SpawnSequencer.SetPhase(2);

    // No more UFO spawns either
    UFOSpawner.disabled = false;

    // Disable navmesh agents to stop movement
    NavMeshAgent[] agents = Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
    foreach (NavMeshAgent agent in agents)
    {
      agent.enabled = false;
    }

    // Disable enemy controllers to stop attacking and sounds
    EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
    foreach (EnemyController enemy in enemies)
    {
      enemy.enabled = false;
    }
  }
}
