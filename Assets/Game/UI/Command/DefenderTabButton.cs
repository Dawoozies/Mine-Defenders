using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderTabButton : MonoBehaviour
{
    public GameObject commandMenu;
    RectTransform commandMenuRect;
    PlayerControls input;
    RectTransform rectTransform;
    Vector3[] rectCorners = new Vector3[4];
    Vector3[] commandRectCorners = new Vector3[4];
    public delegate void TabToggled(bool menuActive);
    public event TabToggled onTabToggled;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        commandMenuRect = commandMenu.GetComponent<RectTransform>();
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            Vector2 screenPosition = input.ReadValue<Vector2>();
            rectTransform.GetWorldCorners(rectCorners);
            if(commandMenu.activeSelf)
            {
                commandMenuRect.GetWorldCorners(commandRectCorners);
                bool inCommandMenu =
                    (screenPosition.x >= commandRectCorners[0].x && screenPosition.x <= commandRectCorners[2].x)
                    && (screenPosition.y >= commandRectCorners[0].y && screenPosition.y <= commandRectCorners[2].y);
                if(inCommandMenu)
                {
                    return;
                }
            }

            bool outsideBounds =
                (screenPosition.x < rectCorners[0].x || screenPosition.x > rectCorners[2].x)
                || (screenPosition.y < rectCorners[0].y || screenPosition.y > rectCorners[2].y);
            if(outsideBounds)
            {
                commandMenu.SetActive(false);
                onTabToggled?.Invoke(false);
                return;
            }

            commandMenu.SetActive(!commandMenu.activeSelf);
            onTabToggled?.Invoke(commandMenu.activeSelf);
        };
        input.Enable();
    }
}