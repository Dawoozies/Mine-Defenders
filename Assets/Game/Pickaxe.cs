using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Pickaxe : MonoBehaviour
{
    ObjectAnimator objectAnimator;
    SpriteRenderer pickaxeRenderer;
    //This should handle
    private void Start()
    {
        pickaxeRenderer = GetComponent<SpriteRenderer>();
        objectAnimator = GetComponent<ObjectAnimator>();
    }
    public void ToggleMiningEffect(bool isMining)
    {
        if(isMining)
        {
            if(objectAnimator.GetCurrentAnimationName() != "Mining")
                objectAnimator.PlayAnimation("Mining");
            pickaxeRenderer.color = Color.white;
        }
        else
        {
            objectAnimator.StopAnimation();
            pickaxeRenderer.color = Color.clear;
        }
    }
}