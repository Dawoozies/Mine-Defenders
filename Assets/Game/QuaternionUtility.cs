using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class QuaternionUtility
{
    public static Quaternion Conjugate(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }
    public static Quaternion Add(Quaternion lhs, Quaternion rhs)
    {
        return new Quaternion(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
    }
    public static Quaternion Conditional(Quaternion q, Quaternion p, bool condition)
    {
        if(condition)
            return q;

        return p;
    }
}