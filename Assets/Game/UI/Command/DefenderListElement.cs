using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DefenderListElement : MonoBehaviour, IButtonLayout
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
    public ObjectAnimator tabAnimator;
    public SpriteAnimator tabBorderSpriteAnimator;
    Image portraitImage;
    bool dead;
    DefenderTabButton defenderTabButton;
    bool barsDisplayed;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainAnimator = GetComponent<ObjectAnimator>();
        barLayoutRectTransform = barLayoutAnimator.GetComponent<RectTransform>();
        defenderTabButton = GetComponentInChildren<DefenderTabButton>();
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
            if (defender.defenderData.health == 0)
                return;
            barLayoutAnimator.PlayAnimation("ShowBars");
        };
        barLayoutAnimator.onAnimationComplete += () =>
        {
            bars = barLayoutRectTransform.GetComponentsInChildren<Bar>();
            if (bars == null || bars.Length == 0)
                return;
            foreach (Bar bar in bars)
            {
                if (bar.barType == BarType.Health)
                {
                    bar.maxValue = defender.defenderData.maxHealth;
                    bar.minValue = 0;
                    bar.ChangeValue(defender.defenderData.health);
                }
                if (bar.barType == BarType.Exp)
                {
                    bar.maxValue = defender.defenderData.maxExp;
                    bar.minValue = 0;
                    bar.ChangeValue(defender.defenderData.exp);
                }
            }
            barsDisplayed = true;
        };
        mainAnimator.onAnimationComplete += () => {
            SpriteAnimation portraitAnimation = new SpriteAnimation();
            portraitAnimation.animName = "Default";
            portraitAnimation.frames = 2;
            portraitAnimation.sprites = new List<Sprite>() { defender.defenderData.defaultSprites[0], defender.defenderData.defaultSprites[1] };
            portraitAnimation.spriteColors = new List<Color>() { Color.white, Color.white };
            portraitAnimator.CreateAndPlayAnimation(portraitAnimation);
        };
        defenderTabButton.onTabToggled += (bool menuActive) => {
            if (dead)
                return;
            if (menuActive)
            {
                tabBorderSpriteAnimator.PlayAnimation("Selected");
            }
            if (!menuActive && tabBorderSpriteAnimator.GetCurrentAnimationName() == "Selected")
            {
                tabBorderSpriteAnimator.PlayAnimation("Alive");
            }
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

        if (portraitImage == null)
            portraitImage = portraitAnimator.GetComponent<Image>();
        portraitImage.color = Color.clear;
    }
    public void SetElementDefender(Defender defender)
    {
        this.defender = defender;
    }
    private void Update()
    {
        if (defender == null) return;
        if (defender.defenderData.health == 0 && !dead)
        {
            tabAnimator.PlayAnimation("Dead");
            tabBorderSpriteAnimator.PlayAnimation("Dead");
            deathIndicator.SetActive(true);
            barLayoutAnimator.PlayAnimation("HideBars");
            dead = true;
        }
        if (defender.defenderData.health > 0 && dead)
        {
            tabAnimator.PlayAnimation("Alive");
            tabBorderSpriteAnimator.PlayAnimation("Alive");
            deathIndicator.SetActive(false);
            barLayoutAnimator.PlayAnimation("ShowBars");
            dead = false;
        }
        if(barsDisplayed)
        {
            foreach (Bar bar in bars)
            {
                if (bar.barType == BarType.Health)
                    bar.ChangeValue(defender.defenderData.health);
                if (bar.barType == BarType.Exp)
                    bar.ChangeValue(defender.defenderData.exp);
            }
        }
    }
    void IButtonLayout.ButtonPressed(int buttonPressed)
    {
        if (buttonPressed == 0)
        {
            //GameManager.ins.Place_Defender(((IAgent)GameManager.ins.agentController.player).args.cellPos, defender);
            GameManager.PlaceDefenderRequest += () => { return new GameManager.PlaceDefenderRequestArgs(defender, ((IAgent)GameManager.ins.agentController.player).args.cellPos); };
        }
    }
    void OnDisable()
    {
        barsDisplayed = false;
    }
}