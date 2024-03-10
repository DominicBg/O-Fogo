using System.Collections.Generic;
using Unity.Mathematics;

namespace OFogo
{
    public class GroupChildrenAnimation : AnimationAutomation
    {
        List<AnimationAutomation> animations = new List<AnimationAutomation>();

        public override void OnStart()
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].OnStart();
            }
        }

        public override void UpdateAnimation(float timeRatio)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].UpdateAnimation(timeRatio);
            }
        }

        public override void OnEnd()
        {
            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].OnEnd();
            }
        }

        private void Awake()
        {
            GetComponentsInChildren(animations);
            animations.Remove(this);

            for (int i = 0; i < animations.Count; i++)
            {
                duration = math.max(duration, animations[i].duration);
                animations[i].gameObject.SetActive(false);
            }
        }
    }
}