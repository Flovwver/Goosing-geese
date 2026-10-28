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

        if (AssetDatabase.LoadAssetAtPath<Mesh>(_outputAssetPath) != null)
        {
            Debug.LogError("Mirror destination already exists: " + _outputAssetPath, this);
            return;
        }

        string directory = System.IO.Path.GetDirectoryName(_outputAssetPath);

        if (string.IsNullOrEmpty(directory) || AssetDatabase.IsValidFolder(directory) == false)
        {
            Debug.LogError("Mirror destination folder does not exist.", this);
            return;
        }

        Mesh mirroredMesh = new MirroredMeshFactory().Create(_sourceMesh);

        AssetDatabase.CreateAsset(mirroredMesh, _outputAssetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"MeshMirrorTool: saved mirrored mesh to {_outputAssetPath}");
#endif
    }
}
