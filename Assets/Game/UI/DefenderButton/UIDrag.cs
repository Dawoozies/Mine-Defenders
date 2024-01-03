using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UIDrag : Draggable
{
    RectTransform rectTransform;
    Vector3[] rectCorners;
    Vector2 pivotOffset => rectTransform.sizeDelta * rectTransform.pivot;
    Vector2 originalAnchoredPosition;
    public delegate void DragStartEvent(Vector2 pos);
    public event DragStartEvent onDragStart;
    public delegate void WhileDragEvent(Vector2 pos);
    public event WhileDragEvent onWhileDrag;
    public delegate void DragEndEvent(Vector2 pos);
    public event DragEndEvent onDragEnd;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectCorners = new Vector3[4];
    }
    public override void OnDragStart()
    {
        //base.OnDragStart();
        originalAnchoredPosition = rectTransform.anchoredPosition;
        onDragStart?.Invoke(mouseDragStartPos);
    }
    public override void WhileDrag()
    {
        //base.WhileDrag();
        rectTransform.anchoredPosition = mouseScreenPos;
        onWhileDrag?.Invoke(mouseScreenPos);
    }
    public override void OnDragEnd()
    {
        //base.OnDragEnd();
        //We check if we let go over a valid tile and then place the defender down
        //rectTransform.anchoredPosition = mouseDragStartPos + pivotOffset;
        rectTransform.anchoredPosition = originalAnchoredPosition;
        onDragEnd?.Invoke(mouseDragEndPos);
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
