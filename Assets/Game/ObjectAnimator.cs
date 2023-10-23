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
        public List<Vector3> positions;
        public List<Vector3> eulerAngles;
        [HideInInspector]
        public List<Quaternion> rotations;
        public List<InterpolationType> interpolationTypes;
        public bool loop;
        [HideInInspector]
        public float time;
        public void SetUpRotations()
        {
            rotations = new List<Quaternion>();
            for (int i = 0; i < eulerAngles.Count; i++)
            {
                rotations.Add(Quaternion.Euler(eulerAngles[i]));
            }
        }
    }
    SpriteRenderer spriteRenderer;
    public float animationSpeed;
    public List<ObjectAnimation> animations;
    ObjectAnimation currentAnimation;
    int currentIndex;
    int targetIndex;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            currentIndex = (currentIndex + 1) % currentAnimation.positions.Count;
            targetIndex = (currentIndex + 1) % currentAnimation.positions.Count;
            currentAnimation.time = 0;
        }
        transform.localPosition = Interpolation.Interpolate
            (
                currentAnimation.positions[currentIndex],
                currentAnimation.positions[targetIndex],
                currentAnimation.time,
                currentAnimation.interpolationTypes[currentIndex]
            );
        transform.localRotation = Interpolation.Interpolate
            (
                currentAnimation.rotations[currentIndex],
                currentAnimation.rotations[targetIndex],
                currentAnimation.time,
                currentAnimation.interpolationTypes[currentIndex]
            );
    }
    public string GetCurrentAnimationName()
    {
        if (currentAnimation == null)
            return null;
        return currentAnimation.animName;
    }
}