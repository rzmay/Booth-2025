using UnityEngine;

public class ReverberatingCarbonizerBullet : StandardIssueBullet
{
    protected void ApplyDamage(Collider collider, Vector3 normal)
    {
        base.ApplyDamage(collider, normal);

        // Slow enemy

        // Apply random mutation????
    }
}
