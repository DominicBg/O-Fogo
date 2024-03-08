using Unity.Jobs;

namespace OFogo
{
    public interface ICalentador
    {
        void HeatParticle(ref FireParticle particle);
    }
}