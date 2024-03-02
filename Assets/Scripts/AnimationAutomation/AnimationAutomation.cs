using UnityEngine;

namespace OFogo
{
    public abstract class AnimationAutomation : MonoBehaviour
    {
        public float duration = 1;

        public abstract void OnStart();
        public abstract void UpdateAnimation(float time, float timeRatio);
        public abstract void OnEnd();
    }
}