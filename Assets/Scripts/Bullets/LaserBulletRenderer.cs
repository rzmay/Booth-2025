using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class LaserBulletRenderer : MonoBehaviour
{
    public float length = 1.0f;
    public float width = 1.0f;

    public float plasmaSpread = 0.5f;

    private ParticleSystem _ps;

    [SerializeField]
    private GameObject _cylinder;

    [SerializeField]
    private GameObject _glow;

    void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        _ps = GetComponent<ParticleSystem>();

        RenderCylinder();
    }

    void LateUpdate()
    {
        RenderCylinder();

        // Set particle lifetime
        var main = _ps.main;
        float lifetime = length / _ps.velocityOverLifetime.zMultiplier;
        main.startLifetime = lifetime;
        main.startSize = width * 1.75f;

        // Set particle emission radius
        var shape = _ps.shape;
        shape.radius = plasmaSpread * (width / 2);
    }

    void RenderCylinder()
    {
        // Set cylinder transform
        _cylinder.transform.localScale = new Vector3(width, (length / 2), width);
        _cylinder.transform.localPosition = Vector3.forward * length / 2;

        // Set glow transform and height
        _glow.transform.localScale = new Vector3(1.75f, (2 / length), 1.75f);

        _glow.GetComponent<SpriteRenderer>().size = new Vector2(1, length * 1.1f);
    }
}
