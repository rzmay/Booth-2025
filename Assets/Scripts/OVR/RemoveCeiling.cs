using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoveCeiling : MonoBehaviour
{
  public float ceilingDetectionThreshold = -0.7f; // Normal Y value threshold for detecting ceilings
  public float ceilingHeightExtension = 3f;       // How much to extend ceiling vertices upwards
  public float heightPercentile = 0.95f;          // Top percentage of vertices to consider as ceilings (0.95 = top 5%)

  private MeshFilter _meshFilter;

  private MeshCollider _meshCollider;

  void Start()
  {
    _meshFilter = GetComponent<MeshFilter>();
    _meshCollider = GetComponent<MeshCollider>();
  }

  public void RemoveMeshCeiling()
  {
    if (!isActiveAndEnabled) return;
    Debug.Log("[RemoveCeiling] Removing ceiling...");

    Mesh mesh = CopyMesh(_meshFilter.sharedMesh);
    if (mesh == null) return;

    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;
    Vector3[] normals = mesh.normals;

    HashSet<int> ceilingVertices = new HashSet<int>();
    List<int> newTriangles = new List<int>();

    // Step 1: Determine the height threshold (top 5% of vertices)
    float[] vertexHeights = vertices.Select(v => v.y).ToArray();
    System.Array.Sort(vertexHeights);
    float heightThreshold = vertexHeights[Mathf.FloorToInt(vertexHeights.Length * heightPercentile)];

    Debug.Log("[RemoveCeiling] Removing faces...");
    for (int i = 0; i < triangles.Length; i += 3)
    {
      // Get indices of the triangle's vertices
      int i0 = triangles[i];
      int i1 = triangles[i + 1];
      int i2 = triangles[i + 2];

      // Average normal of the triangle (we'll use this to detect ceilings)
      Vector3 faceNormal = (normals[i0] + normals[i1] + normals[i2]).normalized;

      // Calculate average height of the face
      float averageHeight = (vertices[i0].y + vertices[i1].y + vertices[i2].y) / 3f;

      // If the normal is pointing down and it's above the height threshold
      if (faceNormal.y < ceilingDetectionThreshold && averageHeight > heightThreshold)
      {
        // Mark the ceiling vertices for extension
        ceilingVertices.Add(i0);
        ceilingVertices.Add(i1);
        ceilingVertices.Add(i2);
      }
      else
      {
        // Keep this triangle if it's not part of the ceiling
        newTriangles.Add(i0);
        newTriangles.Add(i1);
        newTriangles.Add(i2);
      }
    }
    Debug.Log($"[RemoveCeiling] Removed {triangles.Length - newTriangles.Count} triangles");

    Debug.Log($"[RemoveCeiling] Moving ceiling vertices...");
    // Step 2: Extend ceiling vertices upwards
    foreach (int index in ceilingVertices)
    {
      vertices[index] += Vector3.up * ceilingHeightExtension;
    }
    Debug.Log($"[RemoveCeiling] Moving {ceilingVertices} ceiling vertices");


    // Step 3: Apply modified data to the mesh
    Debug.Log("[RemoveCeiling] Adding modified data...");
    mesh.vertices = vertices;
    mesh.triangles = newTriangles.ToArray();
    mesh.RecalculateBounds();
    mesh.RecalculateNormals();

    // Reset mesh filter
    _meshFilter.sharedMesh = null;
    _meshFilter.mesh = mesh;

    // Recalculate mesh collider
    Debug.Log("[RemoveCeiling] Reloading collider...");
    _meshCollider.sharedMesh = null;
    _meshCollider.sharedMesh = _meshFilter.mesh;

    Debug.Log("[RemoveCeiling] Done!");
  }

  Mesh CopyMesh(Mesh mesh)
  {
    return new Mesh()
    {
      vertices = mesh.vertices,
      triangles = mesh.triangles,
      normals = mesh.normals,
      tangents = mesh.tangents,
      bounds = mesh.bounds,
      uv = mesh.uv
    };
  }
}
