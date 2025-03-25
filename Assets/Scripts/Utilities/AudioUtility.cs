using UnityEngine;
using UnityEngine.Audio;

public static class AudioUtility
{
  private static AudioMixerGroup _mixerGroup;

  // Method to initialize the config (optional, but good practice)
  public static void Initialize(AudioMixerGroup mixerGroup)
  {
    _mixerGroup = mixerGroup;
  }

  public static void PlaySpatialClipAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, float pitch = 1.0f)
  {
    // Create a new GameObject at the specified position
    GameObject tempAudioObject = new GameObject("SpatialAudio");
    tempAudioObject.transform.position = position;

    // Add an AudioSource component
    AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();
    audioSource.clip = clip;
    audioSource.volume = 1f;
    audioSource.pitch = pitch;
    audioSource.spatialize = true;
    audioSource.spatialBlend = 1.0f; // Ensure the audio is fully 3D

    // Assign the AudioMixerGroup if provided
    if (_mixerGroup != null)
    {
      audioSource.outputAudioMixerGroup = _mixerGroup;
    }

    // Add the Meta XR Audio Source component
    MetaXRAudioSource metaXRAudioSource = tempAudioObject.AddComponent<MetaXRAudioSource>();
    metaXRAudioSource.GainBoostDb = volume;

    // Play the audio
    audioSource.Play();

    // Destroy the GameObject after the clip has finished playing
    Object.Destroy(tempAudioObject, clip.length);
  }

  public static void PlaySpatialClipAtPointWithVariation(AudioClip clip, Vector3 position, float volume = 1.0f, float pitchVariation = 0.1f)
  {
    float pitch = 1.0f + Random.Range(-1f, 1f) * pitchVariation;
    PlaySpatialClipAtPoint(clip, position, volume, pitch);
  }
}
