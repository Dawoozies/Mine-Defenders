using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class EnemyBase : ScriptableObject
{
    public Color bloodColor;
    public int moveSpeed; //Tiles per second
    public InterpolationType moveInterpolationType;
    public InterpolationType attackInterpolationType;
    public Sprite[] defaultSprites;
}
