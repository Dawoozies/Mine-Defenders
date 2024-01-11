using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour, IDrag
{
    PlayerControls input;
    public Vector2 mouseDragStartPos;
    public Vector2 mouseDragEndPos;
    public Vector2 mouseScreenPos;
    bool isDragging;
    public delegate void DragStartEvent(Vector2 pos);
    public event DragStartEvent onDragStart;
    public delegate void WhileDragEvent(Vector2 pos);
    public event WhileDragEvent onWhileDrag;
    public delegate void DragEndEvent(Vector2 pos);
    public event DragEndEvent onDragEnd;
    void Awake()
    {
        input = new PlayerControls();
        input.ScreenPos.MouseScreenPos.performed += (input) =>
        {
            mouseScreenPos = input.ReadValue<Vector2>();
        };
        input.Player.Press.performed += (input) =>
        {
            mouseDragStartPos = mouseScreenPos;
            if(CheckDragAllowed())
            {
                isDragging = true;
                OnDragStart();
            }
        };
        input.Player.Press.canceled += (input) =>
        {
            mouseDragEndPos = mouseScreenPos;
            if (isDragging)
            {
                isDragging = false;
                OnDragEnd();
            }
        };
        input.Enable();
    }
    void Update()
    {
        if (isDragging)
            WhileDrag();
    }
    public virtual void OnDragStart()
    {
        //Debug.Log("On Drag Start base method called, this should be overwritten");
        onDragStart?.Invoke(mouseDragStartPos);
    }
    public virtual void WhileDrag()
    {
        //Debug.Log("While Drag base method called, this should be overwritten");
        onWhileDrag?.Invoke(mouseScreenPos);
    }
    public virtual void OnDragEnd()
    {
        //Debug.Log("On Drag End base method called, this should be overwritten");
        onDragEnd?.Invoke(mouseDragEndPos);
    }

    public virtual bool CheckDragAllowed()
    {
        //Debug.Log("Check Drag Allowed base method called, this should be overwritten");
        return true;
    }
}
