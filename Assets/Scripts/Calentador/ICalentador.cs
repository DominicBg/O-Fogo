using Unity.Jobs;

namespace OFogo
{
    public interface ICalentador
    {
        void WarmupParticle(ref FireParticle particle);
    }
}