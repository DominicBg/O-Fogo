using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public enum IntegrationType { Euler, Verlet }

    [System.Serializable]
    public struct SimulationSettings
    {
        [Header("Simulation")]
        public IntegrationType integrationType;
        public Bounds simulationBound;
        public int2 hashingGridLength;
        public float resolutionStepRatio;
        public float colisionVelocityResolution;
        public float maxSpeed;
        public float wallBounceIntensity;

        [Header("Heat")]
        public float fireGravity;
        public float temperatureDropPerSec;
        public float temperatureUpwardForce;
        public float maxTemperature;
        public float heatTransferPercent;

        [Header("Bottom heat")]
        public float heatAtBottomRange;
        public float heatAtBottomPerSec;
        [Range(0, 1)]
        public float heatAtBottomNoiseRatio;
        public float heatAtBottomNoiseSize;
        public float heatAtBottomNoiseSpeed;

        public float minParticleSize;
        public float maxParticleSize;
    }
}