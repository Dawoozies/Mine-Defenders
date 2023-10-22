using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tap : MonoBehaviour
{
    PlayerControls input;
    RectTransform parent;
    public List<RectTransform> fills;
    public List<RectTransform> outlines;
    List<RawImage> fillImages;
    List<RawImage> outlineImages;
    float time = 1;
    public float motionDistance = 2;
    Vector3 endVector;
    public Color fillColor;
    public Color outlineColor;
    public InterpolationType fillVectorInterpolation = InterpolationType.EaseOutExp;
    public InterpolationType outlineVectorInterpolation = InterpolationType.EaseOutExp;
    public InterpolationType fillColorInterpolation = InterpolationType.EaseOutExp;
    public InterpolationType outlineColorInterpolation = InterpolationType.EaseInOutElastic;
    private void OnEnable()
    {
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            var touchPos = input.ReadValue<Vector2>();
            parent.position = touchPos;
            time = 0;
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void Start()
    {
        parent = GetComponent<RectTransform>();
        fillImages = new List<RawImage>();
        outlineImages = new List<RawImage>();
        foreach (var item in fills)
        {
            RawImage fillImage = item.GetComponent<RawImage>();
            fillImage.color = Color.clear;
            fillImages.Add(fillImage);
        }
        foreach (var item in outlines) 
        {
            RawImage outlineImage = item.GetComponent<RawImage>();
            outlineImage.color = Color.clear;
            outlineImages.Add(outlineImage);
        }
    }
    void Update()
    {
        if(time < 1)
        {
            for (int index = 0; index < fills.Count; index++)
            {
                float xMultiplier = (index == 0 || index == 3) ? 1.0f : -1.0f;
                float yMultiplier = (index == 2 || index == 3) ? 1.0f : -1.0f;
                endVector.x = xMultiplier * motionDistance;
                endVector.y = yMultiplier * motionDistance;
                fills[index].anchoredPosition = Interpolation.Interpolate(Vector3.zero, endVector, time, fillVectorInterpolation);
                outlines[index].anchoredPosition = Interpolation.Interpolate(Vector3.zero, endVector, time, outlineVectorInterpolation);
                fillImages[index].color = Interpolation.Interpolate(fillColor, Color.clear, time, fillColorInterpolation);
                outlineImages[index].color = Interpolation.Interpolate(outlineColor, Color.clear, time, outlineColorInterpolation);
            }
            time += Time.deltaTime;
        }
    }
}
