using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RectangleDrag : Draggable
{
    RectTransform rectTransform;
    Vector3[] rectCorners;
    Vector2 pivotOffset => rectTransform.sizeDelta * rectTransform.pivot;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectCorners = new Vector3[4];
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
