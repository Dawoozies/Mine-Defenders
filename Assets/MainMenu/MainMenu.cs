using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public ObjectAnimator[] animators;
    public RectTransform[] hooks;
    public RectTransform titleRect;
    public Image titleImage;
    public TextMeshProUGUI titleText;
    public Vector3 titleOffset;
    public ObjectAnimator gameEntranceAnimator;
    public ObjectAnimator cameraAnimator;
    public ObjectAnimator doorAnimator;
    PlayerControls input;
    bool canEnterGame;
    bool enteringGame;
    public RectTransform[] moveUpOnEnterGame;
    private void OnEnable()
    {
        input = new PlayerControls();
        input.Player.Tap.performed += (input) =>
        {
            if(canEnterGame)
            {
                cameraAnimator.PlayAnimation("EnterGame");
                cameraAnimator.LoopCompleteEvent += EnterGameAnimation_LoopComplete;
                doorAnimator.PlayAnimation("EnterGame");
                canEnterGame = false;
                enteringGame = true;
                foreach (var animator in animators)
                {
                    animator.StopAnimation();
                }
            }
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void Start()
    {
        foreach (var animator in animators)
        {
            animator.PlayAnimation("GrappleTitle");
            animator.LoopCompleteEvent += GrappleTitle_LoopComplete;
            animator.LoopCompleteEvent += PullTitleUp_LoopComplete;
        }

        gameEntranceAnimator.PlayAnimation("Float");
        cameraAnimator.LoopCompleteEvent += StopAnimation_LoopComplete;
        doorAnimator.LoopCompleteEvent += StopAnimation_LoopComplete;

        canEnterGame = false;
    }
    private void Update()
    {
        if (enteringGame)
        {
            foreach (var item in moveUpOnEnterGame)
            {
                item.anchoredPosition = item.anchoredPosition + new Vector2(0, 500f * Time.deltaTime);
            }
        }
        titleRect.position = (hooks[0].position + hooks[1].position)/2f + titleOffset;
    }
    void GrappleTitle_LoopComplete(ObjectAnimator animator, string completedAnimationName)
    {
        if (completedAnimationName != "GrappleTitle")
            return;
        Debug.Log("We get here?");
        animator.StopAnimation();
        animator.PlayAnimation("PullTitleUp");
        titleImage.color = Color.white;
        titleText.color = Color.white;
    }
    void PullTitleUp_LoopComplete(ObjectAnimator animator, string completedAnimationName)
    {
        if (completedAnimationName != "PullTitleUp")
            return;

        animator.StopAnimation();
        animator.PlayAnimation("Float");

        canEnterGame = true;
    }
    void StopAnimation_LoopComplete(ObjectAnimator animator, string completedAnimationName)
    {
        animator.StopAnimation();
    }
    void EnterGameAnimation_LoopComplete(ObjectAnimator animator, string completedAnimationName)
    {
        SceneManager.LoadSceneAsync(1);
    }
}
