using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Damageable))]
public class Player : MonoBehaviour
{

    public List<AudioClip> hurtSounds = new();
    private Damageable _damageable;
    private AudioSource _audioSource;

    private DamageEffect _damageEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _damageable = GetComponent<Damageable>();
        _audioSource = GetComponent<AudioSource>();

        _damageEffect = FindFirstObjectByType<DamageEffect>();

        _damageable.onDamage += OnDamage;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDamage(float health, float damage, bool _)
    {
        MenuController.SetHealth(health / _damageable.health);
        _damageEffect.TriggerDamageEffect(damage);

        PlayHurtSound();

        if (health <= 0)
        {
            // Set music and menu
            MusicManager.PlayTrack("lose", 0f);
            MenuController.SetMenu(2);

            // Stop all enemies
            DisableEnemies();

            // Disable guns
            GunManager.DisableGuns();
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

    void PlayHurtSound()
    {
        if (hurtSounds.Count > 0)
        {
            AudioClip clip = hurtSounds[Random.Range(0, hurtSounds.Count)];

            _audioSource?.Stop();
            _audioSource?.PlayOneShot(clip);
        }
    }
}
