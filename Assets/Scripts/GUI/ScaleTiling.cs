using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class ScaleTiling : MonoBehaviour
{
  public int repetitions = 1;              // Total number of tiles across the longer axis
  public bool updateContinuously = true;   // If true, updates every frame (useful in editor)

  private Image image;
  private RectTransform rectTransform;

  private void Awake()
  {
    image = GetComponent<Image>();
    rectTransform = GetComponent<RectTransform>();
  }

  private void Update()
  {
    if (updateContinuously)
      UpdateTiling();
  }

#if UNITY_EDITOR
  private void OnValidate()
  {
    if (!Application.isPlaying)
    {
      Awake();
      UpdateTiling();
    }
  }
#endif

  public void UpdateTiling()
  {
    if (image == null || image.sprite == null || image.type != Image.Type.Tiled)
    {
      Debug.LogWarning("DynamicTilingScaler requires an Image component with a sprite set to Tiled.");
      return;
    }

    rectTransform = rectTransform ?? GetComponent<RectTransform>();
    Vector2 size = rectTransform.rect.size;

    if (size.x <= 0 || size.y <= 0)
      return;

    float spriteWidth = image.sprite.rect.width;
    float spriteHeight = image.sprite.rect.height;

    bool isWider = size.x > size.y;

    float scaleFactor = isWider
        ? (spriteWidth * repetitions) / size.x
        : (spriteHeight * repetitions) / size.y;

    image.pixelsPerUnitMultiplier = scaleFactor;
  }
}
