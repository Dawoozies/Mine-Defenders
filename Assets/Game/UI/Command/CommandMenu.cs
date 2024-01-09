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
    int previousDefenderCount;
    private void OnEnable()
    {
        //List<DefenderData> d = GameManager.ins.defendersLoaded;
        List<DefenderData> defenders = GameManager.ins.defendersLoaded;
        if(defenderListElements == null)
            defenderListElements = new List<DefenderListElement>();
        #region Add new defender list elements to pool
        if(previousDefenderCount < defenders.Count)
        {
            int elementsToAdd = defenders.Count - previousDefenderCount;
            for(int i = 0; i < elementsToAdd; i++)
            {
                GameObject clonedObject = Instantiate(listElementPrefab, defenderListElementsParent);
                DefenderListElement defenderListElement = clonedObject.GetComponent<DefenderListElement>();
                defenderListElements.Add(defenderListElement);
            }
            previousDefenderCount = defenders.Count;
        }
        #endregion
        #region Set up defender list elements
        listElementRectTransform = listElementPrefab.GetComponent<RectTransform>();
        for (int i = 0; i < defenderListElements.Count; i++)
        {
            if (i >= defenders.Count)
            {
                defenderListElements[i].SetElementDefender(null);
                continue;
            }
            defenderListElements[i].SetElementDefender(defenders[i]);
        }
        #endregion
        #region Set up defender list base rect animation
        listBaseAnimator = defenderListBaseRect.GetComponent<ObjectAnimator>();
        chainSpriteAnimator = defenderListBaseRect.GetComponentInChildren<SpriteAnimator>();

        listWidth = defenders.Count*listElementRectTransform.sizeDelta.x;

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
        for (int i = 0; i < defenders.Count; i++)
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
