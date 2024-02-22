using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class SnapshotReaderVectorfieldGenerator : VectorFieldGenerator
    {
        [SerializeField] VectorFieldSnapshot snapShot;

        public override NativeGrid<float3> CreateVectorField(int2 size, in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            NativeGrid<float3> vectorField = snapShot.Deserialize(allocator);
            if(math.all(size != vectorField.Size))
            {
                Debug.LogError($"Trying to deserialize a Vectorfield of dimensions {vectorField.Size}, expected size of {size}");
                return default;
            }
            return vectorField;
        }

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            //
        }
    }
}