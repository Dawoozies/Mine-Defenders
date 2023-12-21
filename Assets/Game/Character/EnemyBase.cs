using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class EnemyBase : ScriptableObject
{
    public int baseHealth;
    public Color bloodColor;
    public float attackChargeTime;
    public int movementPerTurn;
    public float moveInterpolationSpeed;
    public InterpolationType moveInterpolationType;
    public Sprite[] defaultSprites;
    //public AttackBase[] attackBases;
    public float attackRange;
}