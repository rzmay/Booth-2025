using Oculus.VR;
using UnityEngine;

public class DamageEffect : MonoBehaviour
{
  [Range(0, 1)]
  public float damageIntensity = 0.05f;
  public float damagePulseLength = 0.5f;
  public float lerpFactor = 2f;
  public Material damageMaterial;

  private float _intensity = 0f;
  private Damageable _damageable;


  void Start()
  {
    damageMaterial?.SetFloat("_Intensity", 0f);

    _damageable.onDamage += OnDamage;
  }

  void Update()
  {
    // Reduce pulse
    _intensity -= (damageIntensity / damagePulseLength) * Time.deltaTime;

    // Set material intensity
    if (damageMaterial)
    {
      float _visualIntensity = Mathf.Lerp(damageMaterial.GetFloat("_Intensity"), _intensity, lerpFactor * Time.deltaTime);
      damageMaterial.SetFloat("_Intensity", _visualIntensity);
    }
  }

  public void TriggerDamageEffect(float damageAmount)
  {
    // Set peak intensity
    _intensity = Mathf.Clamp01(damageAmount * damageIntensity);
  }

  void OnDamage(float health, float damage, bool _)
  {
    TriggerDamageEffect(damage);
  }
}
