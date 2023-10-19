using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Stick : MonoBehaviour
{
    PlayerControls input;
    Vector2 touchPos;
    RectTransform stickBaseRectTransform;
    public RectTransform stickRectTransform;
    public TextMeshProUGUI textMesh;
    private void OnEnable()
    {
        input = new PlayerControls();
        input.Player.Tap.performed += (input) =>
        {
            touchPos = input.ReadValue<Vector2>();
            //if touchPos is inside the rect size delta
            float xLowerBound = stickBaseRectTransform.position.x - stickBaseRectTransform.sizeDelta.x / 2;
            float xUpperBound = stickBaseRectTransform.position.x + stickBaseRectTransform.sizeDelta.x / 2;
            float yLowerBound = stickBaseRectTransform.position.y - stickBaseRectTransform.sizeDelta.y / 2;
            float yUpperBound = stickBaseRectTransform.position.y + stickBaseRectTransform.sizeDelta.y / 2;
            //textMesh.text = $"X: {xLowerBound}, {xUpperBound}  Y: {yLowerBound}, {yUpperBound}";
            if(touchPos.x > xLowerBound && touchPos.x < xUpperBound && touchPos.y > yLowerBound && touchPos.y < yUpperBound)
            {
                textMesh.text = touchPos.ToString();
                stickRectTransform.position = touchPos;
            }
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private void Start()
    {
        stickBaseRectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        //textMesh.text = stickBaseRectTransform.sizeDelta.ToString();
    }
}
