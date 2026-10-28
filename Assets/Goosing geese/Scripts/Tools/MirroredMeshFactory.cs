using System;
using UnityEngine;

public sealed class MirroredMeshFactory
{
    public Mesh Create(Mesh sourceMesh)
    {
        if (sourceMesh == null)
            throw new ArgumentNullException(nameof(sourceMesh));

        Mesh mirrored = UnityEngine.Object.Instantiate(sourceMesh);
        mirrored.name = sourceMesh.name + " (mirrored)";
        Vector3[] vertices = mirrored.vertices;
        Vector3[] normals = mirrored.normals;
        Vector4[] tangents = mirrored.tangents;

        for (int index = 0; index < vertices.Length; index++)
            vertices[index].x = -vertices[index].x;

        for (int index = 0; index < normals.Length; index++)
            normals[index].x = -normals[index].x;

        for (int index = 0; index < tangents.Length; index++)
        {
            tangents[index].x = -tangents[index].x;
            tangents[index].w = -tangents[index].w;
        }

        mirrored.vertices = vertices;
        mirrored.normals = normals;
        mirrored.tangents = tangents;

        for (int submesh = 0; submesh < mirrored.subMeshCount; submesh++)
        {
            if (mirrored.GetTopology(submesh) != UnityEngine.MeshTopology.Triangles)
                continue;

            int[] indices = mirrored.GetIndices(submesh);

            for (int index = 0; index < indices.Length; index += 3)
                (indices[index + 1], indices[index + 2]) = (indices[index + 2], indices[index + 1]);

            mirrored.SetIndices(indices, MeshTopology.Triangles, submesh);
        }

        mirrored.RecalculateBounds();
        return mirrored;
    }
}
