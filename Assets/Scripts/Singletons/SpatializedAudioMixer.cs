using UnityEngine;
using UnityEngine.Audio;


public class SpatializedAudioMixer : MonoBehaviour
{

    [SerializeField]
    private AudioMixerGroup _mixer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioUtility.Initialize(_mixer);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
