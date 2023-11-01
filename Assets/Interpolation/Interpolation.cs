using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Interpolation thing using the many different easing curves on https://gizma.com/easing/
public static class Interpolation
{
    public static Vector3 Interpolate(Vector3 start, Vector3 end, float x, InterpolationType interpolationType)
    {
        return Vector3.Lerp(start, end, Interpolator(x, interpolationType));
    }
    public static Color Interpolate(Color start, Color end, float x, InterpolationType interpolationType)
    {
        return Color.Lerp(start, end, Interpolator(x, interpolationType));
    }
    public static Quaternion Interpolate(Quaternion start, Quaternion end, float x, InterpolationType interpolationType)
    {
        return Quaternion.Lerp(start, end, Interpolator(x, interpolationType));
    }
    public static Vector2 Interpolate(Vector2 start, Vector2 end, float x, InterpolationType interpolationType)
    {
        return Vector2.Lerp(start, end, Interpolator(x, interpolationType));
    }
    static float Interpolator(float x, InterpolationType interpolationType)
    {
        switch (interpolationType)
        {
            case InterpolationType.Linear:
                return Linear(x);
            case InterpolationType.EaseInElastic:
                return EaseInElastic(x);
            case InterpolationType.EaseOutElastic:
                return EaseOutElastic(x);
            case InterpolationType.EaseInOutElastic:
                return EaseInOutElastic(x);
            case InterpolationType.EaseInExp:
                return EaseInExp(x);
            case InterpolationType.EaseOutExp:
                return EaseOutExp(x);
            case InterpolationType.EaseInOutExp:
                return EaseInOutExp(x);
            case InterpolationType.EaseInOutSine:
                return EaseInOutSine(x);
            default:
                return x;
        }
    }
    static float Linear(float x) 
    {
        return x;
    }
    static float EaseInElastic(float x)
    {
        float c = (2 * Mathf.PI) / 3;
        return x == 0
            ? 0
            : x == 1
            ? 1
            : -Mathf.Pow(2, 10f * x - 10f) * Mathf.Sin((x * 10f - 10.75f) * c);
    }
    static float EaseOutElastic(float x)
    {
        float c = (2 * Mathf.PI) / 3;
        return x == 0
            ? 0
            : x == 1
            ? 1
            : Mathf.Pow(2, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * c)+1;
    }
    static float EaseInOutElastic(float x)
    {
        float c = (2 * Mathf.PI) / 4.5f;
        return x == 0
            ? 0
            : x == 1
            ? 1
            : x < 0.5f
            ? -(Mathf.Pow(2, 20f * x - 10f) * Mathf.Sin((20f * x - 11.125f) * c)) / 2
            : Mathf.Pow(2, -20f * x + 10f) * Mathf.Sin((20f * x - 11.125f * c)) / 2 + 1;
    }
    static float EaseInExp(float x)
    {
        return x == 0 ? 0 : Mathf.Pow(2, 10f * x - 10f);
    }
    static float EaseOutExp(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10f * x);
    }
    static float EaseInOutExp(float x)
    {
        return x == 0
            ? 0
            : x == 1
            ? 1
            : x < 0.5f
            ? Mathf.Pow(2, 20f * x - 10f) / 2
            : (2 - Mathf.Pow(2, -20f * x + 10f)) / 2;
    }
    static float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }
}
public enum InterpolationType
{
    Linear = 0,
    EaseInElastic = 1,
    EaseOutElastic = 2,
    EaseInOutElastic = 3,
    EaseInExp = 4,
    EaseOutExp = 5,
    EaseInOutExp = 6,
    EaseInOutSine = 7
}
