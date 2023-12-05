using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonLayout : MonoBehaviour
{
    PlayerControls input;
    List<RectTransform> commandRects;
    List<Vector3[]> rectCorners;
    IButtonLayout layoutHandler;
    private void OnEnable()
    {
        layoutHandler = GetComponentInParent<IButtonLayout>();
        if(input == null)
        {
            #region Start set up
            commandRects = new List<RectTransform>();
            rectCorners = new List<Vector3[]>();
            //Debug.Log($"transform child count {transform.childCount}");
            for (int i = 0; i < transform.childCount; i++)
            {
                commandRects.Add(transform.GetChild(i).GetComponent<RectTransform>());
                Vector3[] worldCorners = new Vector3[4];
                rectCorners.Add(worldCorners);
            }
            #endregion

            input = new PlayerControls();
            input.Player.Tap.performed += (input) => {
                Vector2 hitPos = input.ReadValue<Vector2>();
                for (int i = 0; i < commandRects.Count; i++)
                {
                    commandRects[i].GetWorldCorners(rectCorners[i]);
                    bool hitButton =
                    (hitPos.x >= rectCorners[i][0].x && hitPos.x <= rectCorners[i][2].x)
                    && (hitPos.y >= rectCorners[i][0].y && hitPos.y <= rectCorners[i][2].y);
                    if (hitButton)
                    {
                        layoutHandler.ButtonPressed(i);
                        break;
                    }
                }
            };
        }
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
}
public interface IButtonLayout
{
    public void ButtonPressed(int buttonPressed);
}