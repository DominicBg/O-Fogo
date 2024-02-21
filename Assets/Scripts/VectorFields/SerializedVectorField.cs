using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class SerializedVectorField : ScriptableObject
    {
        public int2 size;
        public float3[] vectors;

        public void Serialize(in NativeGrid<float3> vectorField)
        {
            size = vectorField.Size;
            vectors = new float3[vectorField.Size.x * vectorField.Size.y]; 
            for (int x = 0; x < vectorField.Size.x; x++)
            {
                for (int y = 0; y < vectorField.Size.y; y++)
                {
                    vectors[vectorField.ToIndex(new int2(x, y))] = vectorField[x, y];
                }
            }
        }
        public NativeGrid<float3> Deserialize(Allocator allocator)
        {
            NativeGrid<float3> vectorField = new NativeGrid<float3>(size, allocator);
            for (int x = 0; x < vectorField.Size.x; x++)
            {
                for (int y = 0; y < vectorField.Size.y; y++)
                {
                    vectorField[x, y] = vectors[vectorField.ToIndex(new int2(x, y))];
                }
            }
            return vectorField;
        }
    }
}