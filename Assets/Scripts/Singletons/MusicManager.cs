using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(DoubleAudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class Track
    {
        public string label;
        public AudioClip audioClip;
    }

    public float volume = 0.5f;
    [SerializeField] public List<Track> trackList = new();

    private DoubleAudioSource _doubleAudioSource;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _doubleAudioSource = GetComponent<DoubleAudioSource>();

        // Play the initial track
        if (trackList.Count > 0) _doubleAudioSource.CrossFade(trackList[0].audioClip, volume, 0f);
    }

    public static void PlayTrack(string label, float fadingTime = 0f)
    {
        Track track = Instance.trackList.Find(t => t.label == label);

        if (track == null) return;

        Instance._doubleAudioSource.CrossFade(track.audioClip, Instance.volume, fadingTime);
    }
}
