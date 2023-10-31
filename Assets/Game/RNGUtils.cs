using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RNGUtils
{
    public System.Random random; 
    public RNGUtils() {
        random = new System.Random();
    }

    public void setSeed(int seed)
    {
        random = new System.Random(seed);
    }

    public int next() {
        return random.Next();
    }

    public Vector2 RandomVector2(int magnitude) {
        return new Vector2((float)random.NextDouble(), (float)random.NextDouble()).normalized * magnitude;
    }
}
