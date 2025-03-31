using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class PrintReceipt : MonoBehaviour
{
  public Camera renderCamera;
  public int targetWidth = 384;
  public int targetHeight = 384;
  public float threshold = 0.5f;
  public bool invert = true;
  public bool flip = false;
  public bool saveToFile = false;
  public string fileName = "RenderedImage";
  public string uploadUrl = "http://192.168.50.115/print";

  [Header("Advanced Options")]
  public bool sendFileHeader = false;
  public float lineChunkHeight = 0f; // 0 means send the full image in one request

  private RenderTexture renderTexture;

  public void Print()
  {
    if (renderCamera == null)
    {
      Debug.LogError("Render Camera not assigned!");
      return;
    }

    renderCamera.gameObject.SetActive(true);

    AdjustCameraForResolution();

    renderTexture = new RenderTexture(targetWidth, targetHeight, 24);
    renderCamera.targetTexture = renderTexture;
    renderCamera.Render();
    renderCamera.targetTexture = null;

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

    _ = SendBitmapInChunksAsync(bitmapBytes);
  }

  private void AdjustCameraForResolution()
  {
    if (renderCamera.orthographic)
    {
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
          bytes[byteIndex++] = currentByte;
          currentByte = 0;
          bitIndex = 0;
        }
      }
    }

    if (bitIndex > 0)
    {
      bytes[byteIndex] = currentByte;
    }

    if (flip)
    {
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

    return bytes;
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
    byte[] header = GenerateBmpHeader(bmpBytes.Length);
    byte[] bmpFile = new byte[header.Length + bmpBytes.Length];
    Buffer.BlockCopy(header, 0, bmpFile, 0, header.Length);
    Buffer.BlockCopy(bmpBytes, 0, bmpFile, header.Length, bmpBytes.Length);

    string filePath = $"Assets/PrinterImages/{fileName}.bmp";
    File.WriteAllBytes(filePath, bmpFile);
    Debug.Log($"Saved BMP file to: {filePath}");

    return bmpFile;
  }

  private byte[] GenerateBmpHeader(int pixelDataLength)
  {
    byte[] fileHeader = new byte[14];
    fileHeader[0] = (byte)'B';
    fileHeader[1] = (byte)'M';
    int fileSize = 14 + 40 + 8 + pixelDataLength;
    BitConverter.GetBytes(fileSize).CopyTo(fileHeader, 2);
    fileHeader[10] = 14 + 40 + 8;

    byte[] dibHeader = new byte[40];
    BitConverter.GetBytes(40).CopyTo(dibHeader, 0);
    BitConverter.GetBytes(targetWidth).CopyTo(dibHeader, 4);
    BitConverter.GetBytes(targetHeight).CopyTo(dibHeader, 8);
    BitConverter.GetBytes((short)1).CopyTo(dibHeader, 12);
    BitConverter.GetBytes((short)1).CopyTo(dibHeader, 14);
    BitConverter.GetBytes(pixelDataLength).CopyTo(dibHeader, 20);

    byte[] palette = new byte[8] { 0, 0, 0, 0, 255, 255, 255, 0 };

    byte[] header = new byte[fileHeader.Length + dibHeader.Length + palette.Length];
    Buffer.BlockCopy(fileHeader, 0, header, 0, fileHeader.Length);
    Buffer.BlockCopy(dibHeader, 0, header, fileHeader.Length, dibHeader.Length);
    Buffer.BlockCopy(palette, 0, header, fileHeader.Length + dibHeader.Length, palette.Length);

    return header;
  }

  public async Task SendBitmapInChunksAsync(byte[] bmpBytes)
  {
    int bytesPerRow = targetWidth / 8;
    int rowsPerChunk = lineChunkHeight > 0 ? Mathf.FloorToInt(lineChunkHeight) : targetHeight;
    int totalChunks = Mathf.CeilToInt((float)targetHeight / rowsPerChunk);

    for (int chunk = 0; chunk < totalChunks; chunk++)
    {
      int startRow = chunk * rowsPerChunk;
      int endRow = Mathf.Min((chunk + 1) * rowsPerChunk, targetHeight);
      int chunkHeight = endRow - startRow;

      byte[] chunkBytes = new byte[chunkHeight * bytesPerRow];
      Buffer.BlockCopy(bmpBytes, startRow * bytesPerRow, chunkBytes, 0, chunkBytes.Length);

      byte[] dataToSend = sendFileHeader ? Combine(GenerateBmpHeader(chunkBytes.Length), chunkBytes) : chunkBytes;
      string byteString = BytesToCommaSeparatedString(dataToSend);

      string url = $"{uploadUrl}?width={targetWidth}&height={chunkHeight}";

      try
      {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
          byte[] postData = Encoding.UTF8.GetBytes(byteString);
          request.uploadHandler = new UploadHandlerRaw(postData);
          request.downloadHandler = new DownloadHandlerBuffer();
          request.SetRequestHeader("Content-Type", "text/plain");

          Debug.Log($"Uploading chunk {chunk + 1}/{totalChunks}...");
          await request.SendWebRequest();

          if (request.result != UnityWebRequest.Result.Success)
          {
            Debug.LogError($"Chunk {chunk + 1} upload failed: {request.error}");
            break; // stop here or continue depending on your preference
          }
          else
          {
            Debug.Log($"Chunk {chunk + 1} uploaded successfully: {request.downloadHandler.text}");
          }
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"Exception during upload of chunk {chunk + 1}: {ex.Message}");
        break;
      }
    }
  }

  private byte[] Combine(byte[] a, byte[] b)
  {
    byte[] result = new byte[a.Length + b.Length];
    Buffer.BlockCopy(a, 0, result, 0, a.Length);
    Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
    return result;
  }

  private string BytesToCommaSeparatedString(byte[] bytes)
  {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < bytes.Length; i++)
    {
      if (i > 0) sb.Append(", ");
      sb.Append("0x" + bytes[i].ToString("X2"));
    }
    return sb.ToString();
  }
}
