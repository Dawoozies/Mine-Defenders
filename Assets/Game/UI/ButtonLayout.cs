using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLayout : MonoBehaviour
{
    PlayerControls input;
    List<RectTransform> commandRects;
    List<Vector3[]> rectCorners;
    public delegate void LayoutPressed(int buttonPressed);
    public event LayoutPressed onLayoutPressed;
    private void OnEnable()
    {
        if(input == null)
        {
            #region Start set up
            commandRects = new List<RectTransform>();
            rectCorners = new List<Vector3[]>();
            Debug.Log($"transform child count {transform.childCount}");
            for (int i = 0; i < transform.childCount; i++)
            {
                commandRects.Add(transform.GetChild(i).GetComponent<RectTransform>());
                Vector3[] worldCorners = new Vector3[4];
                rectCorners.Add(worldCorners);
            }
            #endregion

            onLayoutPressed += (int buttonPressed) => { Debug.Log($"Button pressed {buttonPressed}"); };
            input = new PlayerControls();
            input.Player.Tap.performed += (input) => {
                Vector2 hitPos = input.ReadValue<Vector2>();
                int buttonPressed = -1;
                for (int i = 0; i < commandRects.Count; i++)
                {
                    commandRects[i].GetWorldCorners(rectCorners[i]);
                    bool hitButton =
                    (hitPos.x >= rectCorners[i][0].x && hitPos.x <= rectCorners[i][2].x)
                    && (hitPos.y >= rectCorners[i][0].y && hitPos.y <= rectCorners[i][2].y);
                    if (hitButton)
                    {
                        buttonPressed = i;
                        break;
                    }
                }
                onLayoutPressed?.Invoke(buttonPressed);
            };
        }
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
}
