using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SmoothNormals : MonoBehaviour
{
    private MeshFilter _meshFilter;

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public void SmoothMeshNormals()
    {
        if (!isActiveAndEnabled) return;

        Mesh mesh = _meshFilter.mesh;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = new Vector3[vertices.Length];
        Dictionary<Vector3, List<int>> vertexMap = new Dictionary<Vector3, List<int>>();

        // Group vertices by their position
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            if (!vertexMap.ContainsKey(vertex))
            {
                vertexMap[vertex] = new List<int>();
            }
            vertexMap[vertex].Add(i);
        }

        // Compute the averaged normals
        foreach (var pair in vertexMap)
        {
            List<int> indices = pair.Value;
            Vector3 averageNormal = Vector3.zero;

            foreach (int index in indices)
            {
                averageNormal += mesh.normals[index];
            }
            averageNormal.Normalize();

            foreach (int index in indices)
            {
                normals[index] = averageNormal;
            }
        }

        // Assign the new normals
        mesh.normals = normals;
    }
}
