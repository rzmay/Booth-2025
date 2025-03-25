using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class Bullet : DelayableMonoBehaviour
{
    // The gun passes the bullet it's damage and speed -- the implementation can choose to ignore this lol
    [NonSerialized]
    public float damage;

    [NonSerialized]
    public float speed;

    [SerializeField]
    protected DecalProjector decal;

    protected void ApplyDamage(Collider collider, Vector3 normal, float damageRatio = 1f)
    {
        GameObject other = collider.gameObject;

        // Find the rigidbody correlated with this collider
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb)
        {
            float mass = GetComponent<Rigidbody>()?.mass ?? 1;
            float momentum = mass * speed;
            float force = momentum * damage;


            Vector3 direction = other.transform.position - transform.position;
            rb.AddForceAtPosition(direction.normalized * force, transform.position);
        }

        // Get damageable on rigidbody
        Damageable enemy = rb?.gameObject?.GetComponent<Damageable>();
        if (enemy)
        {
            enemy.Damage(damage * damageRatio, collider);
        }

        // As long as what we hit wasn't an enemy, add the decal
        // if (!enemy && decal)
        // {
        //     // Rotate randomly around z axis
        //     Quaternion rotation = Quaternion.Euler(
        //         -normal.x,
        //         -normal.y,
        //         UnityEngine.Random.Range(0f, 360f)
        //     );

        //     // Instantiate the decal
        //     Instantiate(decal.gameObject, transform.position, rotation);
        // }
    }
}
