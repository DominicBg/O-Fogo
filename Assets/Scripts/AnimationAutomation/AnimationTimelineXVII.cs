using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class AnimationTimelineXVII
{
    public List<AnimationXVII> AnimationParts => animations;
    public EaseXVII.Ease defaultEase = EaseXVII.Ease.InOutQuad;
    public float defaultDuration = 1;

    List<AnimationXVII> animations = new List<AnimationXVII>();

    public AnimationTimelineXVII Wait(float seconds)
    {
        animations.Add(new WaitAnimationPart(seconds));
        return this;
    }
    public AnimationTimelineXVII Add(AnimationXVII animation)
    {
        animation.duration = defaultDuration;
        animation.easeCurve = defaultEase;
        animation.OnCreated();
        animations.Add(animation);
        return this;
    }

    public AnimationTimelineXVII AddGroup(params AnimationXVII[] animations)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].duration = defaultDuration;
            animations[i].easeCurve = defaultEase;
            animations[i].OnCreated();
        }
        this.animations.Add(new GroupAnimationPart(animations.ToList()));
        return this;
    }

    public AnimationTimelineXVII SetAnimationCurve(AnimationCurve animationCurve)
    {
        AnimationXVII animation = animations[animations.Count - 1];
        animation.animationCurve = animationCurve;
        animation.useAnimationCurve = true;
        return this;
    }

    public AnimationTimelineXVII SetEaseCurve(EaseXVII.Ease easeCurve)
    {
        AnimationXVII animation = animations[animations.Count - 1];
        animation.easeCurve = easeCurve;
        animation.useAnimationCurve = false; return this;
    }

    public AnimationTimelineXVII SetDuration(float duration)
    {
        animations[animations.Count - 1].duration = duration;
        return this;
    }

    public AnimationTimelineXVII AddOnStart(Action action)
    {
        animations.Add(new OnStartAction(action));
        return this;
    }

    public AnimationTimelineXVII AddOnEnd(Action action)
    {
        animations.Add(new OnEndAction(action));
        return this;
    }

    public AnimationTimelineXVII AddOnUpdate(Action<float> action)
    {
        animations.Add(new OnUpdateAction(action));
        return this;
    }

    public AnimationTimelineXVII AddTimeLine(AnimationTimelineXVII otherTimeline)
    {
        animations.AddRange(otherTimeline.AnimationParts);
        return this;
    }
}

public interface AnimationTimelineFactory
{
    void CreateAnimationTimeLine(AnimationTimelineXVII timeLine);
}

public abstract class AnimationXVII
{
    public float duration = 1;
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public EaseXVII.Ease easeCurve;
    public bool useAnimationCurve = false;

    public abstract void OnStart();
    public abstract void UpdateAnimation(float timeRatio);
    public abstract void OnEnd();
    public virtual void OnCreated() { }
}

public class WaitAnimationPart : AnimationXVII
{
    public WaitAnimationPart(float duration)
    {
        this.duration = duration;
    }

    public override void OnEnd()
    {
    }

    public override void OnStart()
    {
    }

    public override void UpdateAnimation(float timeRatio)
    {
    }
}

public class GroupAnimationPart : AnimationXVII
{
    List<AnimationXVII> animations;

    public GroupAnimationPart(List<AnimationXVII> animations)
    {
        this.animations = animations;
        for (int i = 0; i < animations.Count; i++)
        {
            duration = math.max(duration, animations[i].duration);
        }
    }

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
}

public class OnStartAction : AnimationXVII
{
    Action action;
    public OnStartAction(Action action)
    {
        this.action = action;
        duration = 0;
    }

    public override void OnEnd()
    {
    }

    public override void OnStart()
    {
        action.Invoke();
    }

    public override void UpdateAnimation(float timeRatio)
    {
    }
}

public class OnUpdateAction : AnimationXVII
{
    Action<float> action;
    public OnUpdateAction(Action<float> action)
    {
        this.action = action;
        duration = 0;
    }

    public override void OnEnd()
    {
    }

    public override void OnStart()
    {
    }

    public override void UpdateAnimation(float timeRatio)
    {
        action.Invoke(timeRatio);
    }
}

public class OnEndAction : AnimationXVII
{
    Action action;
    public OnEndAction(Action action)
    {
        this.action = action;
        duration = 0;
    }

    public override void OnEnd()
    {
        action.Invoke();
    }

    public override void OnStart()
    {
    }

    public override void UpdateAnimation(float timeRatio)
    {
    }
}

public class AnimationTimelineController
{
    List<AnimationXVII> animations;
    bool cycleAnimations;
    AnimationTimelineXVII timeline;
    private float internalTimer;
    private int currentAnimationIndex = 0;
    public bool IsRunning { get; private set; }
    public float TotalDuration { get; private set; }

    public AnimationTimelineController(AnimationTimelineFactory factory)
    {
        timeline = new AnimationTimelineXVII();
        factory.CreateAnimationTimeLine(timeline);

        IsRunning = false;
        animations = timeline.AnimationParts;

        TotalDuration = 0;
        for (int i = 0; i < animations.Count; i++)
        {
            TotalDuration += animations[i].duration;
        }
    }

    public void Start(bool cycleAnimations = false)
    {
        this.cycleAnimations = cycleAnimations;
        animations[currentAnimationIndex].OnStart();
        IsRunning = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning)
        {
            Debug.LogError("Can't run without using Start()");
            return;
        }

        internalTimer += deltaTime;

        var animationAutomation = animations[currentAnimationIndex];

        float timeRatio = animationAutomation.animationCurve.Evaluate(math.saturate(internalTimer / animationAutomation.duration));
        animationAutomation.UpdateAnimation(timeRatio);

        if (internalTimer > animationAutomation.duration)
        {
            internalTimer -= animationAutomation.duration;
            animationAutomation.OnEnd();

            currentAnimationIndex++;
            if (cycleAnimations)
            {
                currentAnimationIndex = currentAnimationIndex % animations.Count;
            }
            if (currentAnimationIndex >= animations.Count)
            {
                IsRunning = false;
                return;
            }

            animations[currentAnimationIndex].OnStart();
        }
    }
}