using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenu : MonoBehaviour
{
    public GameObject listElementPrefab;
    RectTransform listElementRectTransform;

    float listWidth;
    public RectTransform defenderListBaseRect;
    ObjectAnimator listBaseAnimator;
    public RectTransform menuNameWindow;
    public RectTransform defenderListElementsParent;
    SpriteAnimator chainSpriteAnimator;
    Vector2 listStartPos, listEndPos;
    Vector2 listStartSize, listEndSize;
    Vector3[] defenderListCorners = new Vector3[4];
    //0 = bottom left 1 = top left 2 = top right 3 = bottom right
    List<DefenderListElement> defenderListElements = new List<DefenderListElement>();
    List<Bar> bars = new List<Bar>();
    private void OnEnable()
    {
        //List<DefenderData> d = GameManager.ins.defendersLoaded;
        List<Defender> defenders = GameManager.ins.GetDefenders();
        int defenderCount = defenders.Count;
        listElementRectTransform = listElementPrefab.GetComponent<RectTransform>();
        #region Set up defender list elements
        if(defenderListElements == null || defenderListElements.Count == 0)
        {
            defenderListElements = new List<DefenderListElement>();
            for (int i = 0; i < defenderCount; i++)
            {
                GameObject clonedObject = Instantiate(listElementPrefab, defenderListElementsParent);
                DefenderListElement defenderListElement = clonedObject.GetComponent<DefenderListElement>();
                defenderListElements.Add(defenderListElement);
            }
        }
        else
        {
            foreach (DefenderListElement item in defenderListElements)
            {
                item.rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        #endregion
        #region Set up defender list base rect animation
        listBaseAnimator = defenderListBaseRect.GetComponent<ObjectAnimator>();
        chainSpriteAnimator = defenderListBaseRect.GetComponentInChildren<SpriteAnimator>();

        listWidth = defenderCount*listElementRectTransform.sizeDelta.x;

        listStartPos = 
            new Vector2(
                -menuNameWindow.sizeDelta.x,
                defenderListBaseRect.sizeDelta.y/2
                );
        listEndPos = listStartPos + new Vector2(-listWidth/2, 0);
        listStartSize = new Vector2(0, 100);
        listEndSize = new Vector2(listWidth, 100);
        ObjectAnimation openAnimation = new ObjectAnimation();
        openAnimation.animName = "Open";
        openAnimation.frames = 2;
        openAnimation.anchoredPositions = new List<Vector2>()
        {
            listStartPos,
            listEndPos
        };
        openAnimation.sizeDeltas = new List<Vector2>()
        {
            listStartSize,
            listEndSize
        };
        openAnimation.interpolationTypes = new List<InterpolationType>()
        { 
            InterpolationType.EaseOutElastic,
            InterpolationType.EaseOutElastic
        };
        listBaseAnimator.CreateAndPlayAnimation(openAnimation);
        chainSpriteAnimator.PlayAnimation("ChainMoveLeft");
        listBaseAnimator.onAnimationComplete += chainSpriteAnimator.StopAnimation;
        for (int i = 0; i < defenderCount; i++)
        {
            defenderListElements[i].SetElementDefender(defenders[i]);
            //Debug.Log($"Defender = {i}");
        }
        listBaseAnimator.onAnimationComplete += () => 
        {
            defenderListElementsParent.GetWorldCorners(defenderListCorners);
            foreach (var item in defenderListElements)
            {
                item.MoveListElementAlong(defenderListCorners);
            }
        };
        #endregion
    }
    private void OnDisable()
    {
        defenderListBaseRect.anchoredPosition = listStartPos;
        defenderListBaseRect.sizeDelta = listStartSize;
        foreach (var item in defenderListElements)
        {
            item.Reset();
        }
    }
}
