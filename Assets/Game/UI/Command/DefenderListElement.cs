using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DefenderListElement : MonoBehaviour
{
    [HideInInspector]
    public RectTransform rectTransform;
    ObjectAnimator mainAnimator;
    public ObjectAnimator[] gearAnimators;
    public GameObject deathIndicator;
    public ObjectAnimator barLayoutAnimator;
    RectTransform barLayoutRectTransform;
    Defender defender;
    Bar[] bars;
    public SpriteAnimator portraitAnimator;
    Image portraitImage;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainAnimator = GetComponent<ObjectAnimator>();
        barLayoutRectTransform = barLayoutAnimator.GetComponent<RectTransform>();
    }
    public void MoveListElementAlong(Vector3[] listCorners)
    {
        Vector3 listLeftMidpoint = (listCorners[0] + listCorners[1]) / 2;
        Vector3 listRightMidpoint = (listCorners[2] + listCorners[3]) / 2;
        ObjectAnimation moveAlong = new ObjectAnimation();
        moveAlong.animName = "MoveAlong";
        moveAlong.frames = 2;
        moveAlong.useWorldPosition = true;
        moveAlong.positions = new List<Vector3>()
            {
                listRightMidpoint,
                listLeftMidpoint + (rectTransform.sizeDelta.x*(rectTransform.GetSiblingIndex()+0.5f))*Vector3.right
            };
        moveAlong.interpolationTypes = new List<InterpolationType>()
            {
                InterpolationType.EaseInOutElastic,
                InterpolationType.EaseInOutElastic
            };
        mainAnimator.CreateAndPlayAfterTime(moveAlong, 0.02f * rectTransform.GetSiblingIndex());
        foreach (var item in gearAnimators)
        {
            item.PlayAnimation("Rotate");
            mainAnimator.onAnimationComplete += item.StopAnimation;
        }
        mainAnimator.onAnimationComplete += () => {
            barLayoutAnimator.PlayAnimation("ShowBars");
        };
        barLayoutAnimator.onAnimationComplete += () =>
        {
            bars = barLayoutRectTransform.GetComponentsInChildren<Bar>();
            if (bars == null || bars.Length == 0)
                return;
            foreach (Bar bar in bars)
            {
                if(bar.barType == BarType.Health)
                {
                    bar.maxValue = defender.maxHealth;
                    bar.minValue = 0;
                    bar.ChangeValue(defender.health);
                }
                if(bar.barType == BarType.Exp)
                {
                    bar.maxValue = defender.maxExp;
                    bar.minValue = 0;
                    bar.ChangeValue(defender.exp);
                }
            }
        };
        mainAnimator.onAnimationComplete += () => {
            SpriteAnimation portraitAnimation = new SpriteAnimation();
            portraitAnimation.animName = "Default";
            portraitAnimation.frames = 2;
            portraitAnimation.sprites = new List<Sprite>() { defender.defaultSprites[0], defender.defaultSprites[1] };
            portraitAnimation.spriteColors = new List<Color>() { Color.white, Color.white};
            portraitAnimator.CreateAndPlayAnimation(portraitAnimation);
        };
    }
    public void Reset()
    {
        foreach (var item in gearAnimators)
        {
            item.StopAnimation();
        }
        mainAnimator.StopAnimation();
        barLayoutAnimator.StopAnimation();
        portraitAnimator.StopAnimation();
        rectTransform.anchoredPosition = Vector3.zero;
        barLayoutRectTransform.anchoredPosition = new Vector2(-50, 12.5f);
        barLayoutRectTransform.sizeDelta = new Vector2(0, 125f);

        if(portraitImage == null)
            portraitImage = portraitAnimator.GetComponent<Image>();
        portraitImage.color = Color.clear;
    }
    public void SetElementDefender(Defender defender)
    {
        this.defender = defender;

    }
}