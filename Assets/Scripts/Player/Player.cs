using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Damageable))]
public class Player : MonoBehaviour
{
    private Damageable _damageable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _damageable = GetComponent<Damageable>();

        _damageable.onDamage += OnDamage;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDamage(float health, float damage, bool _)
    {
        MenuController.SetHealth(health);

        if (health <= 0)
        {
            // Set music and menu
            MusicManager.PlayTrack("lose", 2f);
            MenuController.SetMenu(2);

            // Stop all enemies
            DisableEnemies();
        }
    }

    void DisableEnemies()
    {
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
