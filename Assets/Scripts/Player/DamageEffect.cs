using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DamageEffect : MonoBehaviour
{
  [Range(0, 1)]
  public float damageIntensity = 0.05f;
  public float damagePulseLength = 0.5f;
  public float lerpFactor = 2f;

  private float _intensity = 0f;
  private RawImage _image;


  void Start()
  {
    _image = GetComponent<RawImage>();
    _image.material?.SetFloat("_Intensity", 0f);
  }

  void Update()
  {
    // Reduce pulse
    _intensity = Mathf.Clamp01(_intensity - (damageIntensity / damagePulseLength) * Time.deltaTime);

    // Set material intensity
    float _visualIntensity = Mathf.Lerp(_image.material.GetFloat("_Intensity"), _intensity, lerpFactor * Time.deltaTime);
    _image.material.SetFloat("_Intensity", _visualIntensity);
  }

  public void TriggerDamageEffect(float damageAmount)
  {
    // Set peak intensity
    _intensity = Mathf.Clamp01(damageAmount * damageIntensity);
  }
}
