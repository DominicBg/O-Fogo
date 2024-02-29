using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class FireStrokeRenderers : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    [SerializeField] int particleCount = 100;
    [SerializeField] float timeScale = 1;
    [SerializeField] float minParticleScale = .5f;
    [SerializeField] float maxParticleScale = 1;
    [SerializeField] float noiseSpeed = 1f;
    [SerializeField] float noiseAmplitude = 0.1f;

    FireStroke[] fireLines;

    NativeArray<ParticleSystem.Particle> renderParticles;
    NativeArray<FireStrokeContainer> fireStrokeContainers;

    //used split evenly all particle based on line length
    NativeArray<ParticleInfoPerLine> particlesInfoPerLines;

    public struct ParticleInfoPerLine
    {
        public float length;
        public int startIndex;
        public int count;
    }

    //todo recieve particle count from controller
    void Start()
    {
        fireLines = GetComponentsInChildren<FireStroke>();

        var main = ps.main;
        main.maxParticles = particleCount;

        var emission = ps.emission;
        emission.enabled = false;

        renderParticles = new NativeArray<ParticleSystem.Particle>(particleCount, Allocator.Persistent);
        fireStrokeContainers = new NativeArray<FireStrokeContainer>(fireLines.Length, Allocator.Persistent);
        particlesInfoPerLines = new NativeArray<ParticleInfoPerLine>(fireLines.Length, Allocator.Persistent);
    }

    void Update()
    {
        float lengthSum = 0;
        for (int i = 0; i < fireLines.Length; i++)
        {
            fireStrokeContainers[i] = fireLines[i].CreateFireStrokeContainer();
            float length = fireStrokeContainers[i].GetLength();
            lengthSum += length;
            particlesInfoPerLines[i] = new ParticleInfoPerLine()
            {
                length = length
            };
        }

        int particleStartIndex = 0;
        for (int i = 0; i < fireLines.Length; i++)
        {
            var info = particlesInfoPerLines[i];
            float lengthRatio = info.length / lengthSum;
            info.count = (int)(lengthRatio * particleCount);

            //with cast to int there might be overflow of particles
            if(particleStartIndex + info.count > particleCount)
            {
                //take the rest
                info.count = particleCount - particleStartIndex;
            }

            info.startIndex = particleStartIndex;
            particlesInfoPerLines[i] = info;

            particleStartIndex += info.count;
        }

        new ProcessFireLineJob()
        {
            timeScale = timeScale,
            minParticleScale = minParticleScale,
            maxParticleScale = maxParticleScale,
            noiseAmplitude = noiseAmplitude,
            noiseSpeed = noiseSpeed,

            time = Time.time,

            particleCount = particleCount,
            renderParticles = renderParticles,
            fireStrokeContainer = fireStrokeContainers,
            particlesInfoPerLines = particlesInfoPerLines
        }.RunParralel(renderParticles.Length);

        ps.SetParticles(renderParticles);
    }

    [BurstCompile]
    public struct ProcessFireLineJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<FireStrokeContainer> fireStrokeContainer;
        [ReadOnly]
        public NativeArray<ParticleInfoPerLine> particlesInfoPerLines;

        public NativeArray<ParticleSystem.Particle> renderParticles;
        public float timeScale;
        public float minParticleScale;
        public float maxParticleScale;
        public float noiseSpeed;
        public float noiseAmplitude;
        public int particleCount;
        public float time;

        public void Execute(int index)
        {
            ParticleInfoPerLine infoPerLine = particlesInfoPerLines[0]; //prevent division per 0
            int fireLineId = 0;
            for (int j = 0; j < particlesInfoPerLines.Length; j++)
            {
                if (index >= particlesInfoPerLines[j].startIndex && index < particlesInfoPerLines[j].startIndex + particlesInfoPerLines[j].count)
                {
                    infoPerLine = particlesInfoPerLines[j];
                    fireLineId = j;
                    break;
                }
            }

            ParticleSystem.Particle particle = renderParticles[index];

            int indexForLine = (index - infoPerLine.startIndex);
            float t = ((float)indexForLine / infoPerLine.count) + time * timeScale;
            t = math.frac(t);
            particle.position = fireStrokeContainer[fireLineId].Evaluate(t);

            float2 noiseValue = noiseAmplitude * new float2(
                noise.snoise(new float2(time * noiseSpeed, index * 1.7283f)),
                noise.snoise(new float2(time * noiseSpeed, index * 7.73816f + particleCount))
            );

            noiseValue = noiseValue * 2 - 1;//normalize [0, 1] -> [-1, 1]

            particle.position += new Vector3(noiseValue.x, noiseValue.y, 0);

            var rng = Unity.Mathematics.Random.CreateFromIndex((uint)index);
            particle.startSize = math.lerp(minParticleScale, maxParticleScale, rng.NextFloat());
            particle.startColor = Color.white * rng.NextFloat();

            renderParticles[index] = particle;
        }
    }

    private void OnDestroy()
    {
        renderParticles.Dispose();
        fireStrokeContainers.Dispose();
        particlesInfoPerLines.Dispose();
    }
}
