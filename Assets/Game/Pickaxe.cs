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
    void Update()
    {

    }
    public void ToggleMiningEffect(bool isMining)
    {
        if(isMining)
        {
            if(objectAnimator.GetCurrentAnimationName() != "Mining")
                objectAnimator.PlayAnimation("Mining");
        }
        else
        {
            objectAnimator.StopAnimation();
        }
    }
}