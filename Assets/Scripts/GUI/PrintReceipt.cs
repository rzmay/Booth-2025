using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PrintReceipt : MonoBehaviour
{
  public Camera renderCamera;                // The camera to render
  public int targetWidth = 384;              // Desired width for the RenderTexture
  public int targetHeight = 384;             // Desired height for the RenderTexture
  public float threshold = 0.5f;             // Threshold for black-and-white conversion (0 - 1)
  public bool invert = true;
  public bool saveToFile = false;            // Flag to save the texture and bitmap
  public string fileName = "RenderedImage";  // Base name for saving files (without extension)
  public string uploadUrl = "http://192.168.50.115/print";  // The URL to POST to

  private RenderTexture renderTexture;

  public void Print()
  {
    if (renderCamera == null)
    {
      Debug.LogError("Render Camera not assigned!");
      return;
    }

    // Set active
    renderCamera.gameObject.SetActive(true);

    AdjustCameraForResolution();

    renderTexture = new RenderTexture(targetWidth, targetHeight, 24);
    renderCamera.targetTexture = renderTexture;
    renderCamera.Render();
    renderCamera.targetTexture = null;

    // Disable camera
    renderCamera.gameObject.SetActive(false);

    Texture2D texture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
    RenderTexture.active = renderTexture;
    texture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
    texture.Apply();
    RenderTexture.active = null;

    if (saveToFile)
    {
      SaveTextureToFile(texture);
    }

    byte[] bitmapBytes = ConvertToBlackAndWhiteBitmap(texture);

    if (saveToFile)
    {
      SaveBitmapToFile(bitmapBytes);
    }

    Destroy(texture);
    renderTexture.Release();

    string byteString = BytesToCommaSeparatedString(bitmapBytes);
    Debug.Log(byteString);
    StartCoroutine(UploadImage(byteString));
  }

  private void AdjustCameraForResolution()
  {
    if (renderCamera.orthographic)
    {
      float aspectRatio = (float)targetWidth / targetHeight;
      renderCamera.orthographicSize = 0.5f * targetHeight / 100f;
    }
    else
    {
      renderCamera.fieldOfView = 60f;
    }
  }

  private byte[] ConvertToBlackAndWhiteBitmap(Texture2D texture)
  {
    int width = texture.width;
    int height = texture.height;

    byte[] bytes = new byte[(width * height) / 8];
    int byteIndex = 0;
    int bitIndex = 0;
    byte currentByte = 0;

    Color[] pixels = texture.GetPixels();

    for (int y = height - 1; y >= 0; y--)
    {
      for (int x = 0; x < width; x++)
      {
        Color pixel = pixels[y * width + x];
        float grayscale = (pixel.r + pixel.g + pixel.b) / 3.0f;

        if (invert ? grayscale < threshold : grayscale > threshold)
        {
          currentByte |= (byte)(1 << (7 - bitIndex));
        }

        bitIndex++;

        if (bitIndex == 8)
        {
          bytes[byteIndex] = currentByte;
          byteIndex++;
          currentByte = 0;
          bitIndex = 0;
        }
      }
    }

    if (bitIndex > 0)
    {
      bytes[byteIndex] = currentByte;
    }

    int bytesPerRow = targetWidth / 8;
    byte[] flippedBmpBytes = new byte[bytes.Length];

    for (int row = 0; row < targetHeight; row++)
    {
      int sourceIndex = row * bytesPerRow;
      int targetIndex = (targetHeight - row - 1) * bytesPerRow;

      Buffer.BlockCopy(bytes, sourceIndex, flippedBmpBytes, targetIndex, bytesPerRow);
    }

    return flippedBmpBytes;
  }

  private void SaveTextureToFile(Texture2D texture)
  {
    byte[] pngBytes = texture.EncodeToPNG();
    string fullPath = $"Assets/PrinterImages/{fileName}.png";
    File.WriteAllBytes(fullPath, pngBytes);
    Debug.Log($"Saved RenderTexture to file: {fullPath}");
  }

  private byte[] SaveBitmapToFile(byte[] bmpBytes)
  {
    // Bitmap file header (14 bytes)
    byte[] fileHeader = new byte[14];
    fileHeader[0] = (byte)'B';
    fileHeader[1] = (byte)'M';

    int fileSize = 14 + 40 + bmpBytes.Length; // File header + info header + pixel data
    fileHeader[2] = (byte)(fileSize);
    fileHeader[3] = (byte)(fileSize >> 8);
    fileHeader[4] = (byte)(fileSize >> 16);
    fileHeader[5] = (byte)(fileSize >> 24);

    fileHeader[10] = 14 + 40;  // Pixel data offset (header size)

    // DIB Header (40 bytes)
    byte[] dibHeader = new byte[40];
    dibHeader[0] = 40;  // DIB header size
    dibHeader[4] = (byte)(targetWidth);
    dibHeader[5] = (byte)(targetWidth >> 8);
    dibHeader[6] = (byte)(targetWidth >> 16);
    dibHeader[7] = (byte)(targetWidth >> 24);

    dibHeader[8] = (byte)(targetHeight);
    dibHeader[9] = (byte)(targetHeight >> 8);
    dibHeader[10] = (byte)(targetHeight >> 16);
    dibHeader[11] = (byte)(targetHeight >> 24);

    dibHeader[12] = 1;  // Number of color planes (1)
    dibHeader[14] = 1;  // Bits per pixel (1 for black and white)
    dibHeader[16] = 0;  // Compression method (0 = none)

    int rawBitmapSize = bmpBytes.Length;
    dibHeader[20] = (byte)(rawBitmapSize);
    dibHeader[21] = (byte)(rawBitmapSize >> 8);
    dibHeader[22] = (byte)(rawBitmapSize >> 16);
    dibHeader[23] = (byte)(rawBitmapSize >> 24);

    // Generate a palette (2 colors, black and white)
    byte[] colorPalette = new byte[8];
    colorPalette[0] = 0; // Blue
    colorPalette[1] = 0; // Green
    colorPalette[2] = 0; // Red
    colorPalette[3] = 0; // Reserved

    colorPalette[4] = 255; // Blue
    colorPalette[5] = 255; // Green
    colorPalette[6] = 255; // Red
    colorPalette[7] = 0; // Reserved

    // Combine all parts into a final byte array
    byte[] bmpFile = new byte[fileHeader.Length + dibHeader.Length + colorPalette.Length + bmpBytes.Length];
    Buffer.BlockCopy(fileHeader, 0, bmpFile, 0, fileHeader.Length);
    Buffer.BlockCopy(dibHeader, 0, bmpFile, fileHeader.Length, dibHeader.Length);
    Buffer.BlockCopy(colorPalette, 0, bmpFile, fileHeader.Length + dibHeader.Length, colorPalette.Length);
    Buffer.BlockCopy(bmpBytes, 0, bmpFile, fileHeader.Length + dibHeader.Length + colorPalette.Length, bmpBytes.Length);

    string filePath = $"Assets/PrinterImages/{fileName}.bmp";
    File.WriteAllBytes(filePath, bmpFile);
    Debug.Log($"Saved BMP file to: {filePath}");

    return bmpFile;
  }

  private string BytesToCommaSeparatedString(byte[] bytes)
  {
    StringBuilder sb = new StringBuilder();

    for (int i = 0; i < bytes.Length; i++)
    {
      if (i > 0)
      {
        sb.Append(", ");
      }
      sb.Append("0x" + bytes[i].ToString("X2"));
    }

    return sb.ToString();
  }

  private System.Collections.IEnumerator UploadImage(string byteString)
  {
    Debug.Log("Ay we runit");
    string url = $"{uploadUrl}?width={targetWidth}&height={targetHeight}";

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
      request.useHttpContinue = true;

      byte[] postData = Encoding.UTF8.GetBytes(byteString);

      request.uploadHandler = new UploadHandlerRaw(postData);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "text/plain");

      Debug.Log("Uploading image...");
      yield return request.SendWebRequest();

      if (request.result == UnityWebRequest.Result.Success)
      {
        Debug.Log("Image uploaded successfully: " + request.downloadHandler.text);
      }
      else
      {
        Debug.LogError("Upload failed: " + request.error);
      }
    }
  }
}
