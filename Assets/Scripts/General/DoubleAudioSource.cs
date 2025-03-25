using UnityEngine;

public class DoubleAudioSource : MonoBehaviour
{
  [SerializeField] AudioSource _source0;
  [SerializeField] AudioSource _source1;

  bool cur_is_source0 = true;

  AudioSource curActiveSource;
  AudioSource newActiveSource;

  float fadeDuration = 0f;
  float fadeTimer = 0f;
  float targetVolume = 1f;

  bool isFading = false;

  void Update()
  {
    // Constantly check if audio sources need to be reinitialized
    if (_source0 == null || _source1 == null)
    {
      InitAudioSources();
    }

    if (isFading)
    {
      FadeUpdate();
    }
  }

  void InitAudioSources()
  {
    AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();

    if (ReferenceEquals(audioSources, null) || audioSources.Length == 0)
    {
      _source0 = gameObject.AddComponent<AudioSource>();
      _source1 = gameObject.AddComponent<AudioSource>();
      return;
    }

    if (audioSources.Length == 1)
    {
      _source0 = audioSources[0];
      _source1 = gameObject.AddComponent<AudioSource>();
    }
    else
    {
      _source0 = audioSources[0];
      _source1 = audioSources[1];
    }
  }

  public void CrossFade(AudioClip clipToPlay, float maxVolume, float fadingTime, float delayBeforeCrossFade = 0)
  {
    if (delayBeforeCrossFade > 0)
    {
      Invoke(nameof(DelayedFadeStart), delayBeforeCrossFade);
    }
    else
    {
      StartFade(clipToPlay, maxVolume, fadingTime);
    }
  }

  void DelayedFadeStart()
  {
    StartFade(null, targetVolume, fadeDuration);
  }

  private void StartFade(AudioClip playMe, float maxVolume, float fadingTime)
  {
    fadeDuration = fadingTime;
    fadeTimer = 0f;
    targetVolume = maxVolume;
    isFading = true;

    if (cur_is_source0)
    {
      curActiveSource = _source0;
      newActiveSource = _source1;
    }
    else
    {
      curActiveSource = _source1;
      newActiveSource = _source0;
    }

    if (playMe != null)
    {
      newActiveSource.clip = playMe;
      newActiveSource.volume = 0f;
      newActiveSource.Play();
    }

    cur_is_source0 = !cur_is_source0;
  }

  private void FadeUpdate()
  {
    if (!isFading) return;

    fadeTimer += Time.deltaTime;
    float progress = Mathf.Clamp01(fadeTimer / fadeDuration);

    if (curActiveSource != null)
      curActiveSource.volume = Mathf.Lerp(targetVolume, 0, progress);

    if (newActiveSource != null)
      newActiveSource.volume = Mathf.Lerp(0, targetVolume, progress);

    if (progress >= 1f)
    {
      isFading = false;
      if (curActiveSource != null)
        curActiveSource.Stop();
    }
  }

  public bool isPlaying
  {
    get
    {
      if (_source0 == null || _source1 == null)
        return false;

      return _source0.isPlaying || _source1.isPlaying;
    }
  }
}
