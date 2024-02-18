using Unity.Mathematics;

namespace OFogo
{
    public struct FireParticle
    {
        public float3 position;
        public float3 prevPosition;//verlet integration
        public float3 velocity;//euler integration
        public float temperature;
        public float radius;
    }
}