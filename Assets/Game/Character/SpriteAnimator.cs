using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SpriteAnimation
{
    public string animName;
    public int frames;
    public List<Sprite> sprites;
    public List<int> spriteOrders;
    public List<Color> spriteColors;
    [HideInInspector]
    public float time;
}
public class SpriteAnimator : MonoBehaviour
{
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
    public void CreateAndPlayAnimation(SpriteAnimation animation)
    {
        if (animations == null || animations.Count == 0)
        {
            animations = new List<SpriteAnimation> { animation };
            PlayAnimation(animations[0].animName);
            return;
        }

        for (int i = 0; i < animations.Count; i++)
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
        if(currentAnimation.spriteOrders != null && currentAnimation.spriteOrders.Count > 0)
        {
            spriteRenderer.sortingOrder = currentAnimation.spriteOrders[currentIndex];
        }
        if(currentAnimation.spriteColors != null && currentAnimation.spriteColors.Count > 0)
        {
            spriteRenderer.color = currentAnimation.spriteColors[currentIndex];
        }
    }
}
