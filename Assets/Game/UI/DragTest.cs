using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragTest : MonoBehaviour
{
    PlayerControls input;
    Vector2 screenPos;
    RectTransform rectTransform;
    bool isDragging;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        input = new PlayerControls();
        input.ScreenPos.MouseScreenPos.performed += (input) =>
        {
            screenPos = input.ReadValue<Vector2>();
        };
        input.Player.Press.performed += (input) =>
        {
            //Debug.LogError("Performed Press");
            isDragging = true;
        };
        input.Player.Press.canceled += (input) =>
        {
            Debug.LogError("Cancelled Press");
            isDragging = false;
        };
        input.Enable();
    }
    private void Update()
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition = screenPos;
        }
    }
}
