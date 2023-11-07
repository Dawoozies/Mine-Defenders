using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ObjectAnimation
{
    public string animName;
    public int frames;
    public List<Vector3> positions;
    public List<Vector3> eulerAngles;
    [HideInInspector]
    public List<Quaternion> rotations;
    public List<Vector2> sizeDeltas;
    public List<Vector2> anchoredPositions;
    public List<InterpolationType> interpolationTypes;
    public bool loop;
    [HideInInspector]
    public float time;
    public void SetUpRotations()
    {
        if (eulerAngles == null || eulerAngles.Count == 0)
            return;
        rotations = new List<Quaternion>();
        for (int i = 0; i < eulerAngles.Count; i++)
        {
            rotations.Add(Quaternion.Euler(eulerAngles[i]));
        }
    }
}
public class IndexProgress
{
    int lastIndex;
    float threshold;
    public delegate void OnThresholdReached();
    public event OnThresholdReached onThresholdReached;
    public IndexProgress(float threshold)
    {
        this.threshold = threshold;
    }
    public void Update(float time, int currentIndex)
    {
        if(time >= threshold && currentIndex != lastIndex)
        {
            lastIndex = currentIndex;
            onThresholdReached?.Invoke();
        }
    }
}
public class ObjectAnimator : MonoBehaviour
{

    RectTransform rectTransform;
    public string playOnStartAnim;
    public float animationSpeed;
    public List<ObjectAnimation> animations;
    ObjectAnimation currentAnimation;
    int currentIndex;
    int targetIndex;
    public delegate void LoopCompleteHandler(ObjectAnimator animator, string completedAnimationName);
    public event LoopCompleteHandler LoopCompleteEvent;
    public delegate void CurrentIndexChangedHandler(ObjectAnimator animator, int currentIndex, string animationName);
    public event CurrentIndexChangedHandler CurrentIndexChangedEvent;
    public delegate void TimeUpdateHandler(float currentTime, int currentIndex, string animationName);
    public event TimeUpdateHandler TimeUpdateEvent;
    IndexProgress Progress_ThreeQuarters;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        foreach (var animation in animations)
        {
            animation.SetUpRotations();
        }

        Progress_ThreeQuarters = new IndexProgress(0.75f);
        Progress_ThreeQuarters.onThresholdReached += () => {
            if (currentAnimation == null)
                return;
            //Debug.Log($"Index Progress: Current Animation Time = {currentAnimation.time}");
            TimeUpdateEvent?.Invoke(currentAnimation.time, currentIndex, currentAnimation.animName);
        };

        if(playOnStartAnim.Length > 0)
        {
            PlayAnimation(playOnStartAnim);
        }
    }
    public void CreateAndPlayAnimation(ObjectAnimation animation)
    {
        if (animation.eulerAngles != null && animation.eulerAngles.Count > 0)
            animation.SetUpRotations();

        if(animations == null || animations.Count == 0)
        {
            animations = new List<ObjectAnimation> { animation };
            PlayAnimation(animations[0].animName);
            return;
        }

        for (int i = 0;  i < animations.Count; i++)
        {
            if (animations[i].animName == animation.animName)
            {
                //Then we should just change the args of the animation that already has the same name
                //instead of adding a new one
                animations[i] = animation;
                PlayAnimation(animations[i].animName);
                return;
            }
        }

        //If we got here then the animation does not exist in the list
        animations.Add(animation);
        PlayAnimation(animation.animName);
    }
    public void PlayAnimation(string animName)
    {
        foreach (var animation in animations)
        {
            if(animation.animName == animName)
            {
                currentAnimation = animation;
                currentIndex = 0;
                targetIndex = 1;
                currentAnimation.time = 0;
            }
        }
    }
    public void StopAnimation()
    {
        currentAnimation = null;
    }
    void Update()
    {
        if (currentAnimation == null)
            return;

        currentAnimation.time += Time.deltaTime*animationSpeed;
        Progress_ThreeQuarters.Update(currentAnimation.time, currentIndex);

        if(currentAnimation.time > 1f)
        {
            currentIndex = (currentIndex + 1) % currentAnimation.frames;
            targetIndex = (currentIndex + 1) % currentAnimation.frames;
            currentAnimation.time = 0;

            CurrentIndexChangedEvent?.Invoke(this, currentIndex, currentAnimation.animName);

            if(currentIndex == currentAnimation.frames - 1)
            {
                //This runs exactly after one loop
                //Debug.LogError("Animation Finished One Loop");
                LoopCompleteEvent?.Invoke(this, currentAnimation.animName);
                if(LoopCompleteEvent == null && !currentAnimation.loop)
                {
                    currentAnimation = null;
                }
            }
        }

        if (currentAnimation == null)
            return;

        if(currentAnimation.positions != null && currentAnimation.positions.Count > 0)
        {
            transform.localPosition = Interpolation.Interpolate
                (
                    currentAnimation.positions[currentIndex % currentAnimation.positions.Count],
                    currentAnimation.positions[targetIndex % currentAnimation.positions.Count],
                    currentAnimation.time,
                    currentAnimation.interpolationTypes[currentIndex % currentAnimation.positions.Count]
                );
        }
        if(currentAnimation.rotations != null && currentAnimation.rotations.Count > 0)
        {
            transform.localRotation = Interpolation.Interpolate
                (
                    currentAnimation.rotations[currentIndex % currentAnimation.rotations.Count],
                    currentAnimation.rotations[targetIndex % currentAnimation.rotations.Count],
                    currentAnimation.time,
                    currentAnimation.interpolationTypes[currentIndex % currentAnimation.rotations.Count]
                );
        }
        //Animation is finished when currentIndex = currentAnimation.frames - 1;
        //UI Down Here
        if (rectTransform == null)
            return;

        if(currentAnimation.sizeDeltas != null && currentAnimation.sizeDeltas.Count > 0)
        {
            rectTransform.sizeDelta = Interpolation.Interpolate
                (
                    currentAnimation.sizeDeltas[currentIndex % currentAnimation.sizeDeltas.Count],
                    currentAnimation.sizeDeltas[targetIndex % currentAnimation.sizeDeltas.Count],
                    currentAnimation.time,
                    currentAnimation.interpolationTypes[currentIndex % currentAnimation.sizeDeltas.Count]
                );
        }
        if(currentAnimation.anchoredPositions != null && currentAnimation.anchoredPositions.Count > 0)
        {
            rectTransform.anchoredPosition = Interpolation.Interpolate
                (
                    currentAnimation.anchoredPositions[currentIndex % currentAnimation.anchoredPositions.Count],
                    currentAnimation.anchoredPositions[targetIndex % currentAnimation.anchoredPositions.Count],
                    currentAnimation.time,
                    currentAnimation.interpolationTypes[currentIndex % currentAnimation.anchoredPositions.Count]
                );
        }
    }
    public string GetCurrentAnimationName()
    {
        if (currentAnimation == null)
            return null;
        return currentAnimation.animName;
    }
}