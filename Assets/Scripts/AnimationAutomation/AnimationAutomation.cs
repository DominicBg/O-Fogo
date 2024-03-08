using UnityEngine;
using UnityEngine.Events;

namespace OFogo
{
    public abstract class AnimationAutomation : MonoBehaviour
    {
        public float duration = 1;
        public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public abstract void OnStart();
        public abstract void UpdateAnimation(float timeRatio);
        public abstract void OnEnd();

        public UnityEvent OnStartEvent;
        public UnityEventFloat OnUpdateEvent;
        public UnityEvent OnEndEvent;
    }
    [System.Serializable]
    public class UnityEventFloat : UnityEvent<float>
    {
    }
}