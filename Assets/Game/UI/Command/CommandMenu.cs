using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMenu : MonoBehaviour
{
    public GameObject listElementPrefab;
    RectTransform listElementRectTransform;

    public int defenderCount;
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
    #region Defender List Element Definition
    public class DefenderListElement
    {
        public RectTransform rectTransform;
        public ObjectAnimator mainAnimator;
        public ObjectAnimator[] objectAnimators;
        public DefenderListElement(GameObject clonedObject)
        {
            rectTransform = clonedObject.GetComponent<RectTransform>();
            mainAnimator = clonedObject.GetComponent<ObjectAnimator>();
            objectAnimators = clonedObject.GetComponentsInChildren<ObjectAnimator>();
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
            mainAnimator.CreateAndPlayAfterTime(moveAlong, 0.02f*rectTransform.GetSiblingIndex());
            foreach (var item in objectAnimators)
            {
                item.PlayAnimation("Rotate");
                mainAnimator.onAnimationComplete += item.StopAnimation;
            }
        }
        public void Reset()
        {
            foreach (var item in objectAnimators)
            {
                item.StopAnimation();
            }
            mainAnimator.StopAnimation();
            rectTransform.anchoredPosition = Vector3.zero;
        }
    }
    #endregion
    List<DefenderListElement> defenderListElements = new List<DefenderListElement>();
    private void OnEnable()
    {
        listElementRectTransform = listElementPrefab.GetComponent<RectTransform>();
        #region Set up defender list elements
        if(defenderListElements == null || defenderListElements.Count == 0)
        {
            defenderListElements = new List<DefenderListElement>();
            for (int i = 0; i < defenderCount; i++)
            {
                GameObject clonedObject = Instantiate(listElementPrefab, defenderListElementsParent);
                DefenderListElement defenderListElement = new DefenderListElement(clonedObject);
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
            InterpolationType.EaseOutExp,
            InterpolationType.EaseOutExp
        };
        listBaseAnimator.CreateAndPlayAnimation(openAnimation);
        chainSpriteAnimator.PlayAnimation("ChainMoveLeft");
        listBaseAnimator.onAnimationComplete += chainSpriteAnimator.StopAnimation;
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
