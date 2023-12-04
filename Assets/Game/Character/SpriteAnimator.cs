using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class SpriteAnimation
{
    public string animName;
    public int frames;
    public List<Sprite> sprites;
    public List<int> spriteOrders;
    public List<Color> spriteColors;
    public List<InterpolationType> interpolationTypes;
    [HideInInspector]
    public float time;
}
public class SpriteAnimator : MonoBehaviour
{
    public string playOnStartAnimName;
    public string playOnEnableAnimName;
    public float animationSpeed;
    public List<SpriteAnimation> animations;
    SpriteAnimation currentAnimation;
    SpriteRenderer spriteRenderer;
    Image image;
    int currentIndex;
    int targetIndex;

    public delegate void LoopCompleteHandler(SpriteAnimator animator, string completedAnimationName);
    public event LoopCompleteHandler LoopCompleteEvent;
    public delegate void EndFrameHandler(int frame);
    public event EndFrameHandler EndFrameEvent;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        if(playOnStartAnimName != null && playOnStartAnimName.Length > 0) 
        { 
            PlayAnimation(playOnStartAnimName);
        }
    }
    private void OnEnable()
    {
        if(spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if(image == null)
            image = GetComponent<Image>();

        if (playOnEnableAnimName != null && playOnEnableAnimName.Length > 0)
        {
            PlayOnce(playOnEnableAnimName);
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
    public void CreateAnimation(SpriteAnimation animation)
    {
        if(animations == null || animations.Count == 0)
        {
            animations = new List<SpriteAnimation> { animation };
            return;
        }
        for (int i = 0; i < animations.Count; i++)
        {
            if (animations[i].animName == animation.animName)
            {
                animations[i] = animation;
                return;
            }
        }
        animations.Add(animation);
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
    public void PlayAnimation(int animIndex)
    {
        if (animations == null || animations.Count == 0)
            return;
        if (animIndex >= animations.Count)
            return;
        currentAnimation = animations[animIndex];
        currentIndex = 0;
        targetIndex = 1;
        currentAnimation.time = 0;
    }
    bool playOnce;
    public void PlayOnce(string animName)
    {
        playOnce = true;
        PlayAnimation(animName);
    }
    public void StopAnimation()
    {
        currentAnimation = null;
    }
    bool stopAtNextFrame;
    public void StopAtNextFrame()
    {
        stopAtNextFrame = true;
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
            if(stopAtNextFrame)
            {
                StopAnimation();
                stopAtNextFrame = false;
                return;
            }
            EndFrameEvent?.Invoke(currentIndex);
            currentIndex = (currentIndex + 1) % currentAnimation.frames;
            targetIndex = (currentIndex + 1) % currentAnimation.frames;
            currentAnimation.time = 0;

            if(currentIndex == currentAnimation.frames - 1)
            {
                LoopCompleteEvent?.Invoke(this, currentAnimation.animName);
                if(playOnce)
                {
                    playOnce = false;
                    currentAnimation = null;
                    return;
                }
            }
        }
        if (currentAnimation == null)
            return;
        #region SpriteRenderer Animation
        if (spriteRenderer != null)
        {
            if (currentAnimation.sprites != null && currentAnimation.sprites.Count > 0)
            {
                spriteRenderer.sprite = currentAnimation.sprites[currentIndex];
            }
            if (currentAnimation.spriteOrders != null && currentAnimation.spriteOrders.Count > 0)
            {
                spriteRenderer.sortingOrder = currentAnimation.spriteOrders[currentIndex];
            }
            if (currentAnimation.spriteColors != null && currentAnimation.spriteColors.Count > 0)
            {
                if(currentAnimation.interpolationTypes != null && currentAnimation.interpolationTypes.Count > 0)
                {
                    spriteRenderer.color = Interpolation.Interpolate
                        (
                            currentAnimation.spriteColors[currentIndex % currentAnimation.spriteColors.Count],
                            currentAnimation.spriteColors[targetIndex % currentAnimation.spriteColors.Count],
                            currentAnimation.time,
                            currentAnimation.interpolationTypes[currentIndex % currentAnimation.spriteColors.Count]
                        );
                }
                else
                {
                    spriteRenderer.color = currentAnimation.spriteColors[currentIndex];
                }
            }
        }
        #endregion
        #region Image Animation
        if (image != null)
        {
            if (currentAnimation.sprites != null && currentAnimation.sprites.Count > 0)
            {
                image.sprite = currentAnimation.sprites[currentIndex];
            }
            if (currentAnimation.spriteColors != null && currentAnimation.spriteColors.Count > 0)
            {
                if (currentAnimation.interpolationTypes != null && currentAnimation.interpolationTypes.Count > 0)
                {
                    image.color = Interpolation.Interpolate
                        (
                            currentAnimation.spriteColors[currentIndex % currentAnimation.spriteColors.Count],
                            currentAnimation.spriteColors[targetIndex % currentAnimation.spriteColors.Count],
                            currentAnimation.time,
                            currentAnimation.interpolationTypes[currentIndex % currentAnimation.spriteColors.Count]
                        );
                }
                else
                {
                    image.color = currentAnimation.spriteColors[currentIndex];
                }
            }
        }
        #endregion
    }
}
