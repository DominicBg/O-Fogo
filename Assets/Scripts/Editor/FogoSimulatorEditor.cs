using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using System.IO;
using System.Collections.Generic;

namespace OFogo
{
    [CustomEditor(typeof(FogoSimulator))]
    public class FogoSimulatorEditor : Editor
    {
        const string k_fileName = "VectorField";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var fogoSimulator = (FogoSimulator)target;

            //fileName = EditorGUILayout.TextField("Save File Name", fileName);
            if (GUILayout.Button("Save VectorField"))
            {
                CreateAsset(fogoSimulator.vectorField, k_fileName + GetNextVectorFieldIndex());
            }

            string[] guids = GetSnapshotGuids();
            for (int i = 0; i < guids.Length; i++)
            {
                string fileName = GetFileNameFromGuid(guids[i], out string assetPath);

                if (GUILayout.Button("Load " + fileName))
                {
                    var serializedVectorField = AssetDatabase.LoadAssetAtPath<VectorFieldSnapshot>(assetPath);
                    if (fogoSimulator.vectorField.IsCreated)
                        fogoSimulator.vectorField.Dispose();

                    fogoSimulator.vectorField = serializedVectorField.Deserialize(Unity.Collections.Allocator.Persistent);
                }
            }
        }

        int GetNextVectorFieldIndex()
        {
            string[] guids = GetSnapshotGuids();

            List<int> currentIndices = new List<int>();
            for (int i = 0; i < guids.Length; i++)
            {
                string fileName = GetFileNameFromGuid(guids[i], out _);
                string fileNumber = fileName.Replace(k_fileName, "").Replace(".asset", "");
                int currentIndex = int.Parse(fileNumber);
                currentIndices.Add(currentIndex);
            }

            if(currentIndices.Count == 0)
            {
                return 0;
            }

            currentIndices.Sort();

            int prevIndex = 0;
            for (int i = 0; i < currentIndices.Count; i++)
            {
                bool skippedOneIndex = currentIndices[i] > prevIndex + 1;
                if (skippedOneIndex)
                {
                    return prevIndex + 1;
                }

                prevIndex = currentIndices[i];
            }


            return prevIndex + 1;
        }

        string[] GetSnapshotGuids()
        {
            return AssetDatabase.FindAssets($"t:{nameof(VectorFieldSnapshot)}"); ;
        }
        string GetFileNameFromGuid(string guid, out string assetPath)
        {
            assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return Path.GetFileName(assetPath);
        }

        public static void CreateAsset(in NativeGrid<float3> vectorField, string name)
        {
            string path = "Assets/VectorFields/" + name + ".asset";
            var serializedVectorField = CreateInstance<VectorFieldSnapshot>();
            serializedVectorField.Serialize(vectorField);
            AssetDatabase.CreateAsset(serializedVectorField, path);
        }
    }
}