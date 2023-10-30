using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [Serializable]
    public class SpriteAnimation
    {
        public string animName;
        public int frames;
        public List<Sprite> sprites;
        [HideInInspector]
        public float time;
    }
    public string playOnStartAnimName;
    public float animationSpeed;
    public List<SpriteAnimation> animations;
    SpriteAnimation currentAnimation;
    SpriteRenderer spriteRenderer;
    int currentIndex;

    public delegate void LoopCompleteHandler(SpriteAnimator animator, string completedAnimationName);
    public event LoopCompleteHandler LoopCompleteEvent;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(playOnStartAnimName != null && playOnStartAnimName.Length > 0) 
        { 
            PlayAnimation(playOnStartAnimName);
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
                currentAnimation.time = 0;
            }
        }
    }
    public void StopAnimation()
    {
        currentAnimation = null;
    }
    public string GetCurrentAnimationName()
    {
        if (currentAnimation == null)
            return null;
        return currentAnimation.animName;
    }
    private void Update()
    {
        if (currentAnimation == null)
            return;
        currentAnimation.time += Time.deltaTime * animationSpeed;
        if(currentAnimation.time > 1)
        {
            currentIndex = (currentIndex + 1) % currentAnimation.frames;
            currentAnimation.time = 0;

            if(currentIndex == currentAnimation.frames - 1)
            {
                LoopCompleteEvent?.Invoke(this, currentAnimation.animName);
            }
        }
        if (currentAnimation == null)
            return;

        if(currentAnimation.sprites != null && currentAnimation.sprites.Count > 0)
        {
            spriteRenderer.sprite = currentAnimation.sprites[currentIndex];
        }
    }
}
