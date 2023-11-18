using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class RadialButton : MonoBehaviour
{
    RectTransform rectTransform;
    bool isOn;
    public int buttonNumber;
    RadialMenu menuController;
    public float minRadius;
    public float maxRadius;
    PlayerControls input;
    public SpriteAnimator selectFX_spriteAnimator;
    public ObjectAnimator selectFX_objectAnimator;
    public ObjectAnimator icon_objectAnimator;
    public Color selectedColor;
    Image image;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        menuController = GetComponentInParent<RadialMenu>();
        image = GetComponent<Image>();

        input = new PlayerControls();
        input.Player.Tap.performed += (input) =>
        {
            Vector3 screenPosition = input.ReadValue<Vector2>();
            float x = screenPosition.x - rectTransform.position.x;
            float y = screenPosition.y - rectTransform.position.y;
            //convert to polar coords and check angles etc
            float r = Mathf.Sqrt(x * x + y * y);
            float angle = Mathf.Atan2(y, x)*Mathf.Rad2Deg;
            
            if(r < minRadius || r > maxRadius)
            {
                return;
            }
            if(buttonNumber == 0 || buttonNumber == 1)
            {
                //Top quads
                float angleLowerBound = 90 + 45 * buttonNumber;
                float angleUpperBound = 90 + 45 * (buttonNumber + 1);
                if(angle < angleLowerBound || angle > angleUpperBound)
                {
                    return;
                }
            }
            if(buttonNumber == 2 || buttonNumber == 3)
            {
                //Bottom quads
                float angleLowerBound = -90 - 45 * (4-buttonNumber);
                float angleUpperBound = -90 - 45 * (4 - (buttonNumber + 1));
                if(angle < angleLowerBound || angle > angleUpperBound)
                {
                    return;
                }
            }
            //Debug.Log($"(angle = {angle}, r = {r})");
            menuController.ButtonPressed(this);
        };
        input.Enable();
    }
    private void OnDisable()
    {
        isOn = false;
        image.color = Color.white;
    }
    public void ToggleButton(RadialButton pressedButton)
    {
        if(pressedButton != this)
        {
            isOn = false;
            image.color = Color.white;
        }
        else
        {
            isOn = true;
            image.color = selectedColor;
            selectFX_spriteAnimator.PlayOnce("SelectEffect");
            selectFX_objectAnimator.PlayOnce("SelectEffect");
            icon_objectAnimator.PlayOnce("ShakeX");
        }
    }
}
