using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using System.IO;

namespace OFogo
{
    [CustomEditor(typeof(DrawingVectorField))]
    public class DrawingVectorFieldEditor : Editor
    {
        string fileName = "VectorField1";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var vectorField = (DrawingVectorField)target;

            fileName = EditorGUILayout.TextField("Save File Name", fileName);
            if (GUILayout.Button("Save VectorField"))
            {
                CreateAsset(vectorField.vectorFieldCopy, fileName);
            }

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(SerializedVectorField)}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileName(assetPath);
                if (GUILayout.Button("Load " + fileName))
                {
                    var serializedVectorField = AssetDatabase.LoadAssetAtPath<SerializedVectorField>(assetPath);
                    vectorField.ImposeVectorField(serializedVectorField.Deserialize(Unity.Collections.Allocator.Persistent));
                }
            }
        }

        public static void CreateAsset(in NativeGrid<float3> vectorField, string name)
        {
            //lol 
            string path = "Assets/VectorFields/" + name + ".asset";
            var serializedVectorField = CreateInstance<SerializedVectorField>();
            serializedVectorField.Serialize(vectorField);
            AssetDatabase.CreateAsset(serializedVectorField, path);
        }
    }
}