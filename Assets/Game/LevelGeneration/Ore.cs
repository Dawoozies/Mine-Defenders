using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu]
public class Ore : ScriptableObject
{
    //ore name
    //ore color
    //rarity
    // --if rarer ores roll it will overwrite more common rolls
    //base roll chance
    //neighbour roll chance
    public Color color;
    public int rarity;
    public int durability;
    public Vector2Int stepMinMax;
    public float baseRollChance;
    public bool BaseRoll()
    {
        return baseRollChance > UnityEngine.Random.Range(0f, 1f);
    }

}
