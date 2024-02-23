namespace OFogo
{
    public struct FireParticleCollision
    {
        public int indexA;
        public int indexB;
        public float distSq;

        public FireParticleCollision(int indexA, int indexB, float distSq)
        {
            this.indexA = indexA;
            this.indexB = indexB;
            this.distSq = distSq;
        }
    }
}