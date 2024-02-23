using Unity.Collections;
using UnityEngine;

namespace OFogo
{
    public abstract class Calentador : MonoBehaviour
    {
        public abstract void HeatParticles(in SimulationData simData, ref NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public virtual void DrawDebug(in SimulationData simData, in SimulationSettings settings) { }
    }
}