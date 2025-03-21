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
        Debug.Log($"[Bullet] ApplyDamage called with damageRatio {damageRatio}");

        // Find the rigidbody correlated with this collider
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb)
        {
            float mass = GetComponent<Rigidbody>()?.mass ?? 1;
            float momentum = mass * speed;
            float force = momentum * damage;


            Vector3 direction = other.transform.position - transform.position;
            rb.AddForceAtPosition(direction.normalized * force, transform.position);
            // Debug.Log($"[Bullet] Applied force {force} to object {rb.name}");
        }

        // Get damageable on rigidbody
        Damageable enemy = rb?.gameObject?.GetComponent<Damageable>();
        if (enemy)
        {
            Debug.Log($"[Bullet] Damageable found, applying damage {damage * damageRatio}");
            enemy.Damage(damage * damageRatio, collider);
        }

        // As long as what we hit wasn't an enemy, add the decal
        if (!enemy && decal)
        {
            Debug.Log("[Bullet] Spawning Decal");
            // Rotate randomly around z axis
            Quaternion rotation = Quaternion.Euler(
                -normal.x,
                -normal.y,
                UnityEngine.Random.Range(0f, 360f)
            );

            // Instantiate the decal
            GameObject decalObject = Instantiate(decal.gameObject, transform.position, rotation);
            Debug.Log("[Bullet] Decal Spawned");
        }
    }
}
