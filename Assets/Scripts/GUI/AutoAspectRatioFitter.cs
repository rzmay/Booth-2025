using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage), typeof(AspectRatioFitter))]
public class AutoAspectRatioFitter : MonoBehaviour
{
  private RawImage rawImage;
  private AspectRatioFitter aspectRatioFitter;

  private void Awake()
  {
    rawImage = GetComponent<RawImage>();
    aspectRatioFitter = GetComponent<AspectRatioFitter>();

    UpdateAspectRatio();
  }

  void Update()
  {
    UpdateAspectRatio();
  }

#if UNITY_EDITOR
  private void OnValidate()
  {
    UpdateAspectRatio();
  }
#endif

  private void UpdateAspectRatio()
  {
    if (rawImage == null || aspectRatioFitter == null)
      return;

    if (rawImage.texture == null)
    {
      return;
    }

    // Attempt to get the texture size from the mainTexture if it's available
    Texture texture = rawImage.mainTexture;

    if (texture == null)
    {
      return;
    }

    float aspectRatio = (float)texture.width / texture.height;

    // Apply the aspect ratio to the AspectRatioFitter
    aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    aspectRatioFitter.aspectRatio = aspectRatio;
  }
}
