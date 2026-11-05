using GoosingGeese.Tools;
using UnityEditor;
using UnityEngine;

namespace GoosingGeese.EditorTools
{
    public sealed class MeshMirrorTool : EditorWindow
    {
        [SerializeField]
        private Mesh _source;

        private void OnGUI()
        {
            _source = (Mesh)EditorGUILayout.ObjectField("Source", _source, typeof(Mesh), false);

            using (new EditorGUI.DisabledScope(_source == null))
            {
                if (GUILayout.Button("Mirror and save"))
                    Save();
            }
        }

        [MenuItem("Tools/Goosing Geese/Mirror mesh")]
        public static void Open()
        {
            GetWindow<MeshMirrorTool>("Mirror mesh");
        }

        private void Save()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save mirrored mesh", _source.name + " Mirrored", "asset", "Choose a destination");

            if (string.IsNullOrEmpty(path))
                return;

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                EditorUtility.DisplayDialog("Destination exists", "Choose a new asset path.", "OK");
                return;
            }

            Mesh result = new MirroredMeshFactory().Create(_source);
            AssetDatabase.CreateAsset(result, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = result;
        }
    }
}
