using System.Collections.Generic;
using UnityEngine;

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
            Disabler.DisableAll();

            // Print
            ScoreTracker.Print(false);
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
