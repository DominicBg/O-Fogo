using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class VectorFieldGenerator : MonoBehaviour
    {
        protected bool isInit { get; private set; }

        public bool TryInit(in SimulationSettings settings) 
        {
            if(!isInit)
            {
                OnInit(in settings);
                isInit = true;
                return true;
            }
            return false;
        }

        public virtual void OnInit(in SimulationSettings settings) { }
        public abstract void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings);
        public virtual void Dispose() { }

        public static NativeGrid<float3> CreateVectorField(in SimulationSettings settings, Allocator allocator = Allocator.Persistent)
        {
            return new NativeGrid<float3>(settings.vectorFieldSize, allocator);
        }
    }
}