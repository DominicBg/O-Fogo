using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class SnapshotReaderVectorfieldGenerator : VectorFieldGenerator
    {
        [SerializeField] VectorFieldSnapshot snapShot;

        protected override void OnUpdateVectorField(in SimulationData simData, ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            if (math.all(settings.vectorFieldSize != vectorField.Size))
            {
                Debug.LogError($"Trying to deserialize a Vectorfield of dimensions {vectorField.Size}, expected size of {settings.vectorFieldSize}");
                return;
            }
            snapShot.DeserializeInVectorField(ref vectorField);
        }
    }
}