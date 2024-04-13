using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class AnimationTimelineXVII
{
    public List<AnimationXVII> AnimationParts => animations;
    public AnimationXVII LastAnimation => animations[animations.Count - 1];

    List<AnimationXVII> animations = new List<AnimationXVII>();

    public AnimationTimelineXVII Wait(float seconds)
    {
        animations.Add(new WaitAnimationPart(seconds));
        return this;
    }
    public AnimationTimelineXVII Add(AnimationXVII animation)
    {
        animation.OnCreated();
        animations.Add(animation);
        return this;
    }

    public AnimationTimelineXVII Add(List<AnimationXVII> animations)
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].OnCreated();
        }
        this.animations.Add(new GroupAnimationPart(animations.ToList()));
        return this;
    }

    public AnimationTimelineXVII Add(params AnimationXVII[] animations)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].OnCreated();
        }
        this.animations.Add(new GroupAnimationPart(animations.ToList()));
        return this;
    }

    public AnimationTimelineXVII SetAnimationCurve(AnimationCurve animationCurve)
    {
        LastAnimation.SetAnimationCurve(animationCurve);
        return this;
    }

    public AnimationTimelineXVII SetEaseCurve(EaseXVII.Ease easeCurve)
    {
        LastAnimation.SetEaseCurve(easeCurve);
        return this;
    }

    public AnimationTimelineXVII SetDuration(float duration)
    {
        LastAnimation.SetDuration(duration);
        return this;
    }

    public AnimationTimelineXVII WithDelay(float delay)
    {
        LastAnimation.SetDelay(delay);
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

    public AnimationTimelineXVII WithSubAnimation(params AnimationXVII[] subAnimations)
    {
        LastAnimation.AddSubAnimation(subAnimations.ToList());
        return this;
    }
}

public interface AnimationTimelineFactory
{
    void CreateAnimationTimeLine(AnimationTimelineXVII timeLine);
}

public abstract class AnimationXVII
{
    public float TotalDuration => duration + delay;
    public const float InvalidDuration = -1;

    public float duration = InvalidDuration;
    public float delay = 0;
    public AnimationCurve animationCurve = null;
    public EaseXVII.Ease easeCurve = EaseXVII.Ease.InOutQuad;
    public bool useAnimationCurve = false;
    public bool isSubAnimation;

    private bool isStarted;
    private bool isCompleted;

    public void UpdateAnimation(float timer)
    {
        float timeRatio = math.saturate((timer - delay) / duration);
        if (!isStarted && timeRatio > 0)
        {
            OnStart();
            isStarted = true;
        }

        timeRatio =
           useAnimationCurve ?
           animationCurve.Evaluate(timeRatio) :
           EaseXVII.Evaluate(timeRatio, easeCurve);

        OnUpdateAnimation(timeRatio);

        if (timeRatio >= 1 && !isCompleted)
        {
            OnEnd();
            isCompleted = true;
        }

        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].UpdateAnimation(timer);
        }
    }

    public void Reset()
    {
        isStarted = false;
        isCompleted = false;
        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].Reset();
        }
    }

    protected abstract void OnStart();
    protected abstract void OnUpdateAnimation(float timeRatio);
    protected abstract void OnEnd();
    public virtual void OnCreated() { }

    public AnimationXVII SetDuration(float duration)
    {
        this.duration = duration;
        for (int i = 0; i < subAnimations.Count; i++)
        {
            if(subAnimations[i].duration == InvalidDuration)
            {
                subAnimations[i].duration = math.clamp(duration, 0, duration - subAnimations[i].delay);
            }
            else
            {
                subAnimations[i].duration = math.clamp(subAnimations[i].duration, 0, duration - subAnimations[i].delay);
            }
        }
        return this;
    }
    public AnimationXVII SetDelay(float delay)
    {
        this.delay = delay;
        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].SetDelay(delay);
        }
        return this;
    }

    public AnimationXVII SetAnimationCurve(AnimationCurve animationCurve)
    {
        this.animationCurve = animationCurve;
        useAnimationCurve = true;

        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].SetAnimationCurve(animationCurve);
        }
        return this;
    }
    public AnimationXVII SetEaseCurve(EaseXVII.Ease easeCurve)
    {
        this.easeCurve = easeCurve;
        useAnimationCurve = false;

        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].SetEaseCurve(easeCurve);
        }
        return this;
    }

    public AnimationXVII AddSubAnimation(List<AnimationXVII> subAnimations)
    {
        if (isSubAnimation)
        {
            Debug.LogError("SubAnimations can't contains subanimations");
            return this;
        }

        subAnimations.AddRange(subAnimations);
        for (int i = 0; i < subAnimations.Count; i++)
        {
            subAnimations[i].isSubAnimation = true;
            duration = math.max(duration, subAnimations[i].duration + subAnimations[i].delay);
        }

        return this;
    }

    public List<AnimationXVII> subAnimations = new List<AnimationXVII>();
}

public class WaitAnimationPart : AnimationXVII
{
    public WaitAnimationPart(float duration)
    {
        this.duration = duration;
    }

    protected override void OnEnd()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdateAnimation(float timeRatio)
    {
    }
}

public class GroupAnimationPart : AnimationXVII
{
    public GroupAnimationPart(List<AnimationXVII> animations)
    {
        if (isSubAnimation)
        {
            Debug.LogError("SubAnimations can't contains subanimations");
            return;
        }

        subAnimations.AddRange(animations);
        for (int i = 0; i < subAnimations.Count; i++)
        {
            duration = math.max(duration, TotalDuration);
            subAnimations[i].isSubAnimation = true;
        }
    }

    protected override void OnEnd()
    {
        //handled in subanim
    }

    protected override void OnStart()
    {
        //handled in subanim
    }

    protected override void OnUpdateAnimation(float timeRatio)
    {
        //handled in subanim
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

    protected override void OnEnd()
    {
    }

    protected override void OnStart()
    {
        action.Invoke();
    }

    protected override void OnUpdateAnimation(float timeRatio)
    {
    }
}

public class OnUpdateAction : AnimationXVII
{
    Action<float> action;
    public OnUpdateAction(Action<float> action)
    {
        this.action = action;
    }

    protected override void OnEnd()
    {
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdateAnimation(float timeRatio)
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

    protected override void OnEnd()
    {
        action.Invoke();
    }

    protected override void OnStart()
    {
    }

    protected override void OnUpdateAnimation(float timeRatio)
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
            TryCreateDefaultSettings(animations[i]);
            for (int j = 0; j < animations[i].subAnimations.Count; j++)
            {
                TryCreateDefaultSettings(animations[i].subAnimations[j]);
            }
        }
    }

    void TryCreateDefaultSettings(AnimationXVII animation)
    {
        if(animation.duration == AnimationXVII.InvalidDuration)
        {
            Debug.LogError(animation + " has a unset duration, will be of 0 secs");
            animation.duration = 0;
        }
    }

    public void Start(bool cycleAnimations = false)
    {
        this.cycleAnimations = cycleAnimations;
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
        animationAutomation.UpdateAnimation(internalTimer);

        if (internalTimer > animationAutomation.TotalDuration)
        {
            internalTimer -= animationAutomation.TotalDuration;

            currentAnimationIndex++;
            if (cycleAnimations)
            {
                currentAnimationIndex = currentAnimationIndex % animations.Count;

                bool justReset = currentAnimationIndex == 0;
                if (justReset)
                {
                    for (int i = 0; i < animations.Count; i++)
                    {
                        animations[i].Reset();
                    }
                }
            }
            if (currentAnimationIndex >= animations.Count)
            {
                IsRunning = false;
                return;
            }
        }
    }
}