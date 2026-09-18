using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshMirrorTool : MonoBehaviour
{
    [SerializeField] private Mesh _sourceMesh;
    [SerializeField] private string _outputAssetPath = "Assets/Meshes/MirroredMesh.asset";

    [ContextMenu("Mirror Mesh")]
    public void MirrorAndSave()
    {
#if UNITY_EDITOR
        if (_sourceMesh == null)
        {
            Debug.LogError("MeshMirrorTool: source mesh is not assigned.");
            return;
        }

        Mesh mirroredMesh = MirroredMeshFactory.Create(_sourceMesh);

        AssetDatabase.CreateAsset(mirroredMesh, _outputAssetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"MeshMirrorTool: saved mirrored mesh to {_outputAssetPath}");
#endif
    }
}
