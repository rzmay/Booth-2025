using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(MeshRenderer))]
public class VideoMaterialRenderer : MonoBehaviour
{
    public VideoClip videoClip; // The video clip to play
    public int materialSlot = 0; // The material slot in the MeshRenderer
    public bool playOnAwake = true; // Option to start playing on awake

    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;

    void Start()
    {
        // Get the MeshRenderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // Create a RenderTexture
        _renderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0);
        _renderTexture.Create();

        // Create the material
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        newMaterial.SetTexture("_BaseMap", _renderTexture);

        // Assign the new material to the MeshRenderer
        Material[] materials = meshRenderer.materials;
        materials[materialSlot] = newMaterial;
        meshRenderer.materials = materials;


        // Create and configure the VideoPlayer
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.clip = videoClip;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _renderTexture;
        _videoPlayer.isLooping = true;

        // Assign the RenderTexture to the material
        Material targetMaterial = meshRenderer.materials[materialSlot];
        targetMaterial.SetTexture("_BaseMap", _renderTexture);

        // Start playing from a random time
        float randomTime = Random.Range(0, (float)videoClip.length);
        _videoPlayer.time = randomTime;

        if (playOnAwake)
        {
            _videoPlayer.Play();
        }
    }

    private void OnDestroy()
    {
        // Cleanup RenderTexture
        if (_renderTexture != null)
        {
            _renderTexture.Release();
        }
    }
}
