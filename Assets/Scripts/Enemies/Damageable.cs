using UnityEngine;

public class Damageable : DelayableMonoBehaviour
{

    public float health
    {
        get { return _health; }
        set { SetHealth(value); }
    }

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
        _health = value; // Check for death or whatever idk i'll figure it our

        // Any onDamage logic
    }
}
