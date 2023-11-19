using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextAnimatorTest : MonoBehaviour
{
    [TextArea(10,20)]
    public string textInput;
    public string animToPlay;
    public bool playAnimation;
    public bool playAnimationOnce;
    public bool oneAtATime;
    TextAnimator animator;
    private void Start()
    {
        animator = GetComponent<TextAnimator>();
    }
    void Update()
    {
        if(playAnimation)
        {
            animator.PlayAnimation(animToPlay, textInput, oneAtATime, false);
            playAnimation = false;
        }
        if(playAnimationOnce)
        {
            animator.PlayAnimation(animToPlay, textInput, oneAtATime, true);
            playAnimationOnce = false;
        }
    }
}
