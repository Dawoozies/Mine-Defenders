using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DefenderButton : Draggable
{
    RectTransform rectTransform;
    Vector3[] rectCorners;
    Vector2 pivotOffset => rectTransform.sizeDelta * rectTransform.pivot;
    Vector2 originalAnchoredPosition;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectCorners = new Vector3[4];
    }
    public override void OnDragStart()
    {
        //base.OnDragStart();
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }
    public override void OnDragEnd()
    {
        //base.OnDragEnd();
        //We check if we let go over a valid tile and then place the defender down
        //rectTransform.anchoredPosition = mouseDragStartPos + pivotOffset;
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
    public override void WhileDrag()
    {
        //base.WhileDrag();
        rectTransform.anchoredPosition = mouseScreenPos;
    }
    public override bool CheckDragAllowed()
    {
        rectTransform.GetWorldCorners(rectCorners);
        bool hitButton =
            (mouseDragStartPos.x >= rectCorners[0].x && mouseDragStartPos.x <= rectCorners[2].x)
            && (mouseDragStartPos.y >= rectCorners[0].y && mouseDragStartPos.y <= rectCorners[2].y);
        return hitButton;
    }
}
