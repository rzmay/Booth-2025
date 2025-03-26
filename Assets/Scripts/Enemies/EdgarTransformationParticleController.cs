using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EdgarTransformationParticalController : MonoBehaviour
{
  public AudioClip transformationSound;
  public float soundVolume = 1.0f;
  public ParticleSystem gooParticleSystem;
  public ParticleSystem fliesParticleSystem;

  void Start()
  {
    // No other scripts are just on edgar, so we'll just trigger his theme here
    MusicManager.PlayTrack("boss", 0.5f);
  }

  public void OnTransformationStart()
  {
    Debug.Log($"[{name}:EdgarTransformationParticalController] OnTransformationStart");
    AudioUtility.PlaySpatialClipAtPoint(transformationSound, transform.position, soundVolume);

    gooParticleSystem.Play();
    fliesParticleSystem.Play();
  }
}
