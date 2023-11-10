using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class EnemyBase : ScriptableObject
{
    public Color bloodColor;
    public float attackChargeTime;
    public int moveSpeed; //Tiles per movement update
    public InterpolationType moveInterpolationType;
    public InterpolationType attackInterpolationType;
    public Sprite[] defaultSprites;
}
