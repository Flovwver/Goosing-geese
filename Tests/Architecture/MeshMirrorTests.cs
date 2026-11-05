using System;
using GoosingGeese.Tools;
using UnityEngine;

public static class MeshMirrorTests
{
    public static string Main()
    {
        var source = new Mesh();
        Mesh result = null;

        try
        {
            source.vertices = new[]
            {
                Vector3.right,
                Vector3.up,
                Vector3.forward
            };
            source.normals = new[]
            {
                Vector3.right,
                Vector3.right,
                Vector3.right
            };
            source.tangents = new[]
            {
                new Vector4(1f, 0f, 0f, 1f),
                Vector4.one,
                Vector4.one
            };
            source.uv2 = new[]
            {
                Vector2.zero,
                Vector2.up,
                Vector2.one
            };
            source.colors = new[]
            {
                Color.red,
                Color.green,
                Color.blue
            };
            source.subMeshCount = 2;
            source.SetTriangles(new[] { 0, 1, 2 }, 0);
            source.SetTriangles(new[] { 2, 1, 0 }, 1);
            result = new MirroredMeshFactory().Create(source);
            Require(result.subMeshCount == 2 && result.GetTriangles(0)[1] == 2, "Each submesh must reverse winding independently.");
            Require(result.vertices[0] == Vector3.left && result.normals[0] == Vector3.left, "Vertices and normals must reflect.");
            Require(result.tangents[0].x == -1f && result.tangents[0].w == -1f, "Tangent handedness must reflect.");
            Require(result.uv2[2] == source.uv2[2] && result.colors[1] == Color.green, "Additional channels must survive.");
            Require(source.vertices[0] == Vector3.right && source.GetTriangles(0)[1] == 1, "Source must be untouched.");
            return "PASS: mirrored submeshes, normals, tangent handedness, UV2 and colors; source isolation.";
        }
        finally
        {
            UnityEngine.Object.Destroy(source);
            UnityEngine.Object.Destroy(result);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
