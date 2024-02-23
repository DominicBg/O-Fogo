using Unity.Jobs;

namespace OFogo
{
    public interface ICalentadorJobParralel
    {
        void WarmupParticle(ref FireParticle particle);
    }
}