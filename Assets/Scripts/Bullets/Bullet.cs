using System;
using UnityEngine;

public abstract class Bullet : DelayableMonoBehaviour
{
    // The gun passes the bullet it's damage and speed -- the implementation can choose to ignore this lol
    [NonSerialized]
    public float damage;
    [NonSerialized]

    public float speed;

    protected void ApplyDamage(GameObject other, float damageRatio = 1f)
    {
        Damageable enemy = other.GetComponent<Damageable>();
        if (enemy)
        {
            enemy.health -= damage * damageRatio;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            float mass = GetComponent<Rigidbody>()?.mass ?? 1;
            float momentum = mass * speed;
            float force = momentum * damage;


            Vector3 direction = other.transform.position - transform.position;
            rb.AddForceAtPosition(direction.normalized * force, transform.position);
            Debug.Log($"[Bullet] Applied force {force} to object {rb.name}");
        }
    }
}
