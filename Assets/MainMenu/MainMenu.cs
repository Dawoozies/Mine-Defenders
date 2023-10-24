using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MainMenu : MonoBehaviour
{
    public ObjectAnimator[] animators;
    void Start()
    {
        foreach (var animator in animators)
        {
            animator.PlayAnimation("GrappleTitle");
            animator.LoopCompleteEvent += GrappleTitle_LoopComplete;
        }
    }
    void GrappleTitle_LoopComplete(string completedAnimationName)
    {
        if (completedAnimationName != "GrappleTitle")
            return;

        foreach (var animator in animators)
        {
            animator.StopAnimation();
        }
    }
}
