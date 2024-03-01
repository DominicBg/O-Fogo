namespace OFogo
{
    public struct FireParticleCollision
    {
        public short indexA;
        public short indexB;
        public float distSq;

        public FireParticleCollision(int indexA, int indexB, float distSq)
        {
            this.indexA = (short)indexA;
            this.indexB = (short)indexB;
            this.distSq = distSq;
        }
    }
}