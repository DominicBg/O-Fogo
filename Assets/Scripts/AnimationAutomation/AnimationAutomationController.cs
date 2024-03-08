using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class AnimationAutomationController : MonoBehaviour
    {
        [SerializeField] AnimationAutomation[] animationAutomations;
        [SerializeField] bool cycleAnimations;

        private float internalTimer;
        private int currentAnimationIndex = 0;

        private void Start()
        {
            animationAutomations = GetComponentsInChildren<AnimationAutomation>();
            animationAutomations[currentAnimationIndex].OnStart();
            animationAutomations[currentAnimationIndex].OnStartEvent.Invoke();
        }

        void Update()
        {
            internalTimer += Time.deltaTime;

            var animationAutomation = animationAutomations[currentAnimationIndex];

            float timeRatio = animationAutomation.animationCurve.Evaluate(math.saturate(internalTimer / animationAutomation.duration));
            animationAutomation.UpdateAnimation(timeRatio);

            if (internalTimer > animationAutomation.duration)
            {
                internalTimer -= animationAutomation.duration;
                animationAutomation.OnEnd();
                animationAutomation.OnEndEvent.Invoke();

                currentAnimationIndex++;
                if(cycleAnimations)
                {
                    currentAnimationIndex = currentAnimationIndex % animationAutomations.Length;
                }
                
                if(currentAnimationIndex >= animationAutomations.Length)
                {
                    enabled = false;
                    return;
                }

                animationAutomations[currentAnimationIndex].OnStart();
                animationAutomations[currentAnimationIndex].OnStartEvent.Invoke();
            }
        }
    }
}