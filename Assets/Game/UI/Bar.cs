using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    public BarType barType;
    public RectTransform background;
    public RectTransform fill;
    public int maxValue;
    public int minValue;
    public int value;
    Vector3[] worldCorners = new Vector3[4];

    #region normalized value interpolation
    public float interpolationSpeed;
    public InterpolationType interpolationType;
    float t;
    float normalizedValueTarget;
    float lastNormalizedValueTarget;
    #endregion
    public void ChangeValue(int newValue)
    {
        if (value == newValue)
            return;
        t = 0;
        value = newValue;
        if(maxValue == minValue)
        {
            normalizedValueTarget = 0;
            return;
        }
        lastNormalizedValueTarget = normalizedValueTarget;
        normalizedValueTarget = (float)Mathf.Abs(value - minValue) / Mathf.Abs(maxValue - minValue);
        //Debug.Log($"last value = {lastNormalizedValueTarget} current value = {normalizedValueTarget}");
    }
    private void Update()
    {
        background.GetWorldCorners(worldCorners);
        if(worldCorners == null || worldCorners.Length < 4)
            return;
        float normalizedValue = 0;
        if (t > 1)
            return;
        t += Time.deltaTime*interpolationSpeed;
        if(maxValue != minValue)
        {
            normalizedValue = Interpolation.Interpolate(lastNormalizedValueTarget, normalizedValueTarget, t, interpolationType);
        }
        float size = Mathf.Lerp(0, Mathf.Abs(worldCorners[0].x - worldCorners[3].x), normalizedValue);
        //fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        fill.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, size);
        //Debug.Log(fill.sizeDelta);
    }
    private void OnDisable()
    {
        maxValue = 0;
        minValue = 0;
        value = 0;
        t = 0;
    }
}
public enum BarType
{
    Health = 0,
    Exp = 1,
}