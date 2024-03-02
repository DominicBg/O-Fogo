using System.Collections.Generic;
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
            Debug.Log("Starting anim " + currentAnimationIndex);
        }

        void Update()
        {
            internalTimer += Time.deltaTime;

            var animationAutomation = animationAutomations[currentAnimationIndex];

            animationAutomation.UpdateAnimation(math.min(internalTimer, animationAutomation.duration), math.saturate(internalTimer / animationAutomation.duration));

            if (internalTimer > animationAutomation.duration)
            {
                internalTimer -= animationAutomation.duration;
                animationAutomation.OnEnd();
                Debug.Log("ennding anim " + currentAnimationIndex);

                currentAnimationIndex++;
                if(cycleAnimations)
                {
                    currentAnimationIndex = currentAnimationIndex % animationAutomations.Length;
                }
                
                if(currentAnimationIndex >= animationAutomations.Length)
                {
                    Debug.Log("finalized animations");
                    enabled = false;
                    return;
                }

                animationAutomations[currentAnimationIndex].OnStart();
                Debug.Log("Starting anim " + currentAnimationIndex);
            }
        }
    }
}