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

        public static FireParticle Lerp(in FireParticle a, in FireParticle b, float t)
        {
            return new FireParticle()
            {
                position = math.lerp(a.position, b.position, t),
                prevPosition = math.lerp(a.prevPosition, b.prevPosition, t),
                radius = math.lerp(a.radius, b.radius, t),
                temperature = math.lerp(a.temperature, b.temperature, t),
                velocity = math.lerp(a.velocity, b.velocity, t),
            };
        }
    }
}