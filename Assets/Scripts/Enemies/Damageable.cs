using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Damageable : DelayableMonoBehaviour
{
    [System.Serializable]
    public class HitPoint
    {
        public Collider collider;
        public float multiplier;
    }

    public bool invincible = false;
    public bool damageAfterDeath = false;
    [SerializeField] public List<HitPoint> hitPoints = new();
    public delegate void OnDamage(float health, float damage, bool isCritical);
    public OnDamage onDamage;

    public float health = 10f;

    [System.NonSerialized] public bool dead = false;

    private float _health;

    public float currentHealth
    {
        get { return _health; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set health to max health
        _health = health;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Damage(float damage, Collider collider = null)
    {
        // Early return if damage after death disabled
        if (dead && !damageAfterDeath) return;

        // Early return if invincible
        if (invincible) return;

        bool isCritical = false;
        float value = damage;
        if (collider)
        {
            HitPoint hitPoint = hitPoints.Find(h => h.collider == collider);
            if (hitPoint != null)
            {
                value *= hitPoint.multiplier;
                isCritical = hitPoint.multiplier > 1;
            }
        }

        _health -= damage;
        if (_health <= 0) dead = true;

        Debug.Log($"[{name}] Took {damage} damage, health = {_health} {(isCritical ? "(critical)" : "")}");

        // Invoke callback, then set dead in order to identify post-mortem hits
        onDamage?.Invoke(_health, value, isCritical);
    }
}
