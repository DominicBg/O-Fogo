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
        public IntegrationType integrationType;
        public Bounds simulationBound;
        public int2 hashingGridLength;
        public float resolutionStepRatio;
        public float colisionVelocityResolution;
        public float maxSpeed;
        public float wallBounceIntensity;
        public bool useVectorFieldAsGravity;

        [Header("Heat")]
        public float fireGravity;
        public float temperatureDropPerSec;
        public float temperatureUpwardForce;
        public float maxTemperature;
        public float heatTransferPercent;
        public float heatingPerSec;

        [Header("Particles")]
        public float minParticleSize;
        public float maxParticleSize;
    }
}