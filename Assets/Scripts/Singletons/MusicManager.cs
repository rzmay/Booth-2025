using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DoubleAudioSource))]
public class MusicManager : MonoBehaviour
{
    private static MusicManager _Instance;

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
        _Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _doubleAudioSource = GetComponent<DoubleAudioSource>();

        // Play the initial track
        if (trackList.Count > 0) _PlayTrack(trackList[0].label);
    }

    void _PlayTrack(string label, float fadingTime = 0f)
    {
        Track track = trackList.Find(t => t.label == label);

        if (track == null) return;

        _doubleAudioSource.CrossFade(track.audioClip, volume, fadingTime);
    }

    public static void PlayTrack(string label, float fadingTime = 0f)
    {
        _Instance._PlayTrack(label, fadingTime);
    }
}
