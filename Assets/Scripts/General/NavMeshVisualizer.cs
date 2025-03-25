using UnityEngine;
using UnityEngine.AI;

public class NavMeshVisualizer : MonoBehaviour
{
    public Material navMeshMaterial; // Assign a material in the Inspector
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public void Refresh()
    {
        // Don't render if inactive
        if (!isActiveAndEnabled) return;

        // Get NavMesh data
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        if (navMeshData.vertices.Length == 0 || navMeshData.indices.Length == 0) return;

        // Create a new GameObject for visualization
        GameObject navMeshObj = new GameObject("NavMeshVisualizer");
        navMeshObj.transform.SetParent(transform); // Optional: Attach to this object
        navMeshObj.transform.position = Vector3.zero;

        // Add required components
        meshFilter = navMeshObj.AddComponent<MeshFilter>();
        meshRenderer = navMeshObj.AddComponent<MeshRenderer>();

        // Create the mesh
        Mesh mesh = new Mesh
        {
            vertices = navMeshData.vertices,
            triangles = navMeshData.indices
        };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // Assign material
        if (navMeshMaterial == null)
        {
            navMeshMaterial = new Material(Shader.Find("Standard"));
            navMeshMaterial.color = new Color(0f, 1f, 0f, 0.5f); // Semi-transparent green
        }
        meshRenderer.material = navMeshMaterial;
    }
}
