using UnityEngine;

public static class MirroredMeshFactory
{
    public static Mesh Create(Mesh sourceMesh)
    {
        Vector3[] sourceVertices = sourceMesh.vertices;
        Vector3[] mirroredVertices = new Vector3[sourceVertices.Length];

        for (int vertexIndex = 0; vertexIndex < sourceVertices.Length; vertexIndex++)
        {
            Vector3 vertex = sourceVertices[vertexIndex];
            vertex.x = -vertex.x;
            mirroredVertices[vertexIndex] = vertex;
        }

        Vector3[] sourceNormals = sourceMesh.normals;
        Vector3[] mirroredNormals = new Vector3[sourceNormals.Length];

        for (int normalIndex = 0; normalIndex < sourceNormals.Length; normalIndex++)
        {
            Vector3 normal = sourceNormals[normalIndex];
            normal.x = -normal.x;
            mirroredNormals[normalIndex] = normal;
        }

        int[] sourceTriangles = sourceMesh.triangles;
        int[] mirroredTriangles = new int[sourceTriangles.Length];

        for (int triangleIndex = 0; triangleIndex < sourceTriangles.Length; triangleIndex += 3)
        {
            mirroredTriangles[triangleIndex] = sourceTriangles[triangleIndex];
            mirroredTriangles[triangleIndex + 1] = sourceTriangles[triangleIndex + 2];
            mirroredTriangles[triangleIndex + 2] = sourceTriangles[triangleIndex + 1];
        }

        Mesh mirroredMesh = new Mesh();
        mirroredMesh.vertices = mirroredVertices;
        mirroredMesh.normals = mirroredNormals;
        mirroredMesh.uv = sourceMesh.uv;
        mirroredMesh.triangles = mirroredTriangles;

        return mirroredMesh;
    }
}
