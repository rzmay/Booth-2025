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
    [SerializeField] public List<HitPoint> hitPoints = new();
    public delegate void OnDamage(float health, float damage, bool isCritical);
    public OnDamage onDamage;

    public float health = 10f;

    private float _health;

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

        onDamage?.Invoke(health, value, isCritical);
    }
}
