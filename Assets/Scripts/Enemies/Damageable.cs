using UnityEngine;

public class Damageable : DelayableMonoBehaviour
{
    public delegate void OnDamage(float health, float damage);
    public OnDamage onDamage;

    public float health
    {
        get { return _health; }
        set { SetHealth(value); }
    }

    [SerializeField]
    private float _health = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetHealth(float value)
    {
        float damage = _health - value;
        _health = value;

        Debug.Log($"[Damageable] Took damage {damage}, at health {health}");

        // Any onDamage logic
        onDamage?.Invoke(health, damage);
    }
}
