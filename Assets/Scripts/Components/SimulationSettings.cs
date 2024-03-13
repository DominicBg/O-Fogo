using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public enum IntegrationType { Euler, Verlet }

    [System.Serializable]
    public struct SimulationSettings
    {
        public int particleCount;

        [Header("Simulation")]
        public Bounds simulationBound;

        [Header("Particles")]
        public float maxTemperature;
        public float minParticleSize;
        public float maxParticleSize;

        [Header("Other")]
        public int2 vectorFieldSize;
    }
}