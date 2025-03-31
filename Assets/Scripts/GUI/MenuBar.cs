using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MenuBar : MonoBehaviour
{
  [Header("Anchor Range")]
  [Range(0f, 1f)] public float anchorMinX = 0.1f;
  [Range(0f, 1f)] public float anchorMaxX = 0.2f;
  public float size = 1f; // Multiplier for anchor width

  [Header("Fill Settings")]
  [Range(0f, 1f)] public float fill = 1f;
  public Image fillImage;           // Image with fillAmount
  public RawImage textureImage;     // RawImage for swapping texture

  [Header("Textures")]
  public Texture texture;
  public Texture fullTexture;

  private RectTransform rectTransform;

  private void Awake()
  {
    rectTransform = GetComponent<RectTransform>();
  }

  private void OnEnable()
  {
    UpdateAnchors();
    UpdateVisuals();
  }

  private void LateUpdate()
  {
    UpdateAnchors();
    UpdateVisuals();
  }

  private void UpdateAnchors()
  {
    float baseWidth = anchorMaxX - anchorMinX;
    float newMaxX = anchorMinX + baseWidth * size;

    rectTransform.anchorMin = new Vector2(anchorMinX, rectTransform.anchorMin.y);
    rectTransform.anchorMax = new Vector2(newMaxX, rectTransform.anchorMax.y);
    rectTransform.offsetMin = Vector2.zero;
    rectTransform.offsetMax = Vector2.zero;
  }

  private void UpdateVisuals()
  {
    if (fillImage != null)
    {
      fillImage.fillAmount = fill;
    }

    if (textureImage != null)
    {
      textureImage.texture = (fill >= 1f && fullTexture != null) ? fullTexture : texture;
    }
  }
}
