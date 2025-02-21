using UnityEngine;

public class ReverberatingCarbonizerBullet : StandardIssueBullet
{
    protected void ApplyDamage(GameObject other, Vector3 normal)
    {
        base.ApplyDamage(other, normal);

        // Slow enemy

        // Apply random mutation????
    }
}
