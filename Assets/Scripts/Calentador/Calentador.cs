using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class Calentador : MonoBehaviour
    {
        public abstract void HeatParticles(in SimulationData simData, ref NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public virtual void DrawDebug(float3 simPosition, in SimulationSettings settings) { }
    }
}