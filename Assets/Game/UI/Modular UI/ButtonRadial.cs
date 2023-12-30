using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonRadial : MonoBehaviour, IButton
{
    RectTransform rectTransform;
    public float minRadius;
    public float maxRadius;
    public int buttonNumber;
    PlayerControls input;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            bool buttonPressed = OnTap(input.ReadValue<Vector2>());
            if (buttonPressed)
                ButtonPressed();
        };
        input.Enable();
    }
    public bool OnTap(Vector2 screenPos)
    {
        float x = screenPos.x - rectTransform.position.x;
        float y = screenPos.y - rectTransform.position.y;
        //convert to polar coords and check angles etc
        float r = Mathf.Sqrt(x * x + y * y);
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        if (r < minRadius || r > maxRadius)
        {
            return false;
        }
        if (buttonNumber == 0 || buttonNumber == 1)
        {
            //Top quads
            float angleLowerBound = 90 + 45 * buttonNumber;
            float angleUpperBound = 90 + 45 * (buttonNumber + 1);
            if (angle < angleLowerBound || angle > angleUpperBound)
            {
                return false;
            }
        }
        if (buttonNumber == 2 || buttonNumber == 3)
        {
            //Bottom quads
            float angleLowerBound = -90 - 45 * (4 - buttonNumber);
            float angleUpperBound = -90 - 45 * (4 - (buttonNumber + 1));
            if (angle < angleLowerBound || angle > angleUpperBound)
            {
                return false;
            }
        }
        //Debug.Log($"(angle = {angle}, r = {r})");
        return true;
    }
    public void ButtonPressed()
    {
        UIManager.ins.onRadialButtonPressed += () => { return (buttonNumber, true); };
    }
}
