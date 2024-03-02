using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public static class OFogoHelper
    {
        public static float Pow2(float x) => x * x;

        public static int2 HashPosition(in float3 position, in Bounds bounds, int2 sizes)
        {
            float3 min = bounds.min;
            float3 max = bounds.max;
            float3 positionClamp = math.clamp(position, min, max);
            float3 t = math.unlerp(min, max, positionClamp);
            return math.clamp((int2)(t.xy * sizes), 0, sizes - 1);
        }

        public static int2 Quantize(int2 v, int2 cellSize)
        {
            return new int2(math.floor(v / (float2)cellSize));
        }

        public static void ApplyConstraintBounce(ref FireParticle fireParticles, in SimulationSettings settings)
        {
            if (!settings.simulationBound.Contains(fireParticles.position))
            {
                bool3 outOfBounds = new bool3(
                    fireParticles.position.x < settings.simulationBound.min.x || fireParticles.position.x > settings.simulationBound.max.x,
                    fireParticles.position.y < settings.simulationBound.min.y || fireParticles.position.y < settings.simulationBound.max.y,
                    false
                );

                fireParticles.velocity = math.select(fireParticles.velocity, -fireParticles.velocity * settings.wallBounceIntensity, outOfBounds);
                fireParticles.position = settings.simulationBound.ClosestPoint(fireParticles.position);
            }
        }

        public static void CheckCollisionPairAtPosition(int particleIndex,
            in NativeArray<FireParticle> fireParticles, in NativeGrid<UnsafeList<int>> nativeHashingGrid,
            in SimulationSettings settings, ref NativeList<FireParticleCollision> collisionBuffer, int maxCollision = -1)       
        {
            int2 hash = HashPosition(fireParticles[particleIndex].position, in settings.simulationBound, settings.hashingGridLength);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (maxCollision != -1 && collisionBuffer.Length > maxCollision)
                    {
                        return;
                    }
                    CheckCollisionPair(hash, particleIndex, x, y, fireParticles, nativeHashingGrid, settings, ref collisionBuffer, maxCollision);
                }
            }
        }

        public static void CheckCollisionPair(int2 hash, int i, int x, int y,
            in NativeArray<FireParticle> fireParticles, in NativeGrid<UnsafeList<int>> nativeHashingGrid,
            in SimulationSettings settings, ref NativeList<FireParticleCollision> collisionBuffer, int maxCollision = -1)
        {
            int2 pos = new int2(x + hash.x, y + hash.y);

            if (pos.x < 0 || pos.x >= settings.hashingGridLength.x || pos.y < 0 || pos.y >= settings.hashingGridLength.y)
            {
                return;
            }

            var gridList = nativeHashingGrid[pos.x, pos.y];

            for (int k = 0; k < gridList.Length; k++)
            {
                int j = gridList[k];
                //same particle or already processed the collision when i -> j,  don't need j -> i
                if (i >= j)
                {
                    continue;
                }

                float distSq = math.distancesq(fireParticles[i].position, fireParticles[j].position);
                float radiusSqSum = Pow2(fireParticles[i].radius + fireParticles[j].radius);

                if (distSq < radiusSqSum)
                {
                    collisionBuffer.Add(new FireParticleCollision(i, j, distSq));

                    if(maxCollision != -1 && collisionBuffer.Length > maxCollision)
                    {
                        return;
                    }
                }
            }
        }
    }
}