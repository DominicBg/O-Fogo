using Unity.Collections;
using UnityEngine;

namespace OFogo
{
    public abstract class FogoRenderer : MonoBehaviour
    {
        public abstract void Init(int particleCount);
        public abstract void Render(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public abstract void Dispose();
    }
}