using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EdgarTransformationParticalController : MonoBehaviour
{
  public AudioClip transformationSound;
  public float soundVolume = 1.0f;
  public ParticleSystem gooParticleSystem;
  public ParticleSystem fliesParticleSystem;

  public void OnTransformationStart()
  {
    AudioUtility.PlaySpatialClipAtPoint(transformationSound, transform.position, soundVolume);

    gooParticleSystem.Play();
    fliesParticleSystem.Play();
  }
}
