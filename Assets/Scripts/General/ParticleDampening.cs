using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleDampening : MonoBehaviour
{
    public float amount = 1f;
    public float power = 1.5f;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private Vector3 _lastPos;

    private Vector3 _velocity;

    void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        if (!_ps) _ps = GetComponent<ParticleSystem>();

        // Allocate particle array based on max particles
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];

        // For velocity caluclation
        _lastPos = transform.position;
        _velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        _velocity = (transform.position - _lastPos) / Time.deltaTime;
        _lastPos = transform.position;

        if (!_ps) return;

        int numParticles = _ps.GetParticles(_particles);

        for (int i = 0; i < numParticles; i++)
        {
            // Get particle lifetime percentage (0 = just spawned, 1 = about to die)
            float lifetimePercent = 1 - (_particles[i].remainingLifetime / _particles[i].startLifetime);

            // Calculate dampening factor
            float dampeningFactor = Mathf.Lerp(0, 1, Mathf.Pow(lifetimePercent, power));

            // Compute world-space velocity adjustment
            Vector3 targetVelocity = -_velocity * dampeningFactor * amount;

            // Gradually apply damping
            _particles[i].velocity = transform.InverseTransformDirection(Vector3.Lerp(Vector3.zero, targetVelocity, lifetimePercent));
        }

        // Apply the updated particles back to the system
        _ps.SetParticles(_particles, numParticles);
    }
}
