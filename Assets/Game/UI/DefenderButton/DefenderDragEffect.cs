using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DefenderDragEffect : MonoBehaviour
{
    Draggable drag;
    public UIElement ghost;
    public UIElement main;
    public Sprite ghostSprite;
    public Sprite mainSprite;
    private void Start()
    {
        drag = GetComponent<Draggable>();
        if (ghost != null) ghost.SetUp();
        if(main != null) main.SetUp();
        if (drag == null)
            return;
        ghost.image.color = Color.clear;
        main.image.color = Color.white;
        drag.onDragStart += OnDragStart;
        drag.onWhileDrag += OnWhileDrag;
        drag.onDragEnd += OnDragEnd;
    }
    void OnDragStart(Vector2 pos)
    {

    }
    void OnWhileDrag(Vector2 pos)
    {

    }
    void OnDragEnd(Vector2 pos)
    {

    }
}
