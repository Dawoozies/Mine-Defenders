using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectAnimator : MonoBehaviour
{
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
            if(eulerAngles == null || eulerAngles.Count == 0 ) 
                return;
            rotations = new List<Quaternion>();
            for (int i = 0; i < eulerAngles.Count; i++)
            {
                rotations.Add(Quaternion.Euler(eulerAngles[i]));
            }
        }
    }
    SpriteRenderer spriteRenderer;
    RectTransform rectTransform;
    public float animationSpeed;
    public List<ObjectAnimation> animations;
    ObjectAnimation currentAnimation;
    int currentIndex;
    int targetIndex;

    public delegate void LoopCompleteHandler(ObjectAnimator animator, string completedAnimationName);
    public event LoopCompleteHandler LoopCompleteEvent;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rectTransform = GetComponent<RectTransform>();
        foreach (var animation in animations)
        {
            animation.SetUpRotations();
        }
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
        if(currentAnimation.time > 1)
        {
            currentIndex = (currentIndex + 1) % currentAnimation.frames;
            targetIndex = (currentIndex + 1) % currentAnimation.frames;
            currentAnimation.time = 0;

            if(currentIndex == currentAnimation.frames - 1)
            {
                //This runs exactly after one loop
                //Debug.LogError("Animation Finished One Loop");
                LoopCompleteEvent?.Invoke(this, currentAnimation.animName);
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