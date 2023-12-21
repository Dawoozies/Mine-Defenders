using ItemFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class DefenderBase : ScriptableObject
{
    public int baseHealth;
    public int attackRange;
    public float attackChargeTime;
    public int movementPerTurn;
    public float moveInterpolationSpeed;
    public InterpolationType moveInterpolationType;
    public Sprite[] defaultSprites;
    public GameObject heldWeaponPrefab;
}
[Serializable]
public class DefenderData
{
    public string defenderName;
    public int defenderKey;
    public int maxHealth, health;
    public int maxExp, exp;
    public float attackRange;
    public float attackChargeTime;
    public int movementPerTurn;
    public float moveInterpolationSpeed;
    public InterpolationType moveInterpolationType;
    public Sprite[] defaultSprites;
    //public AttackBase[] attackBases;
    [HideInInspector]
    public GameObject heldWeaponPrefab;
    public DefenderData(DefenderJSON defenderJSON, DefenderBase defenderBase)
    {
        defenderName = defenderJSON.defenderBaseName;
        defenderKey = defenderJSON.defenderKey;
        maxHealth = defenderJSON.maxHealth;
        health = defenderJSON.health;
        maxExp = defenderJSON.maxExp;
        exp = defenderJSON.exp;
        defaultSprites = defenderBase.defaultSprites;
        //attackBases = defenderBase.attackBases;
        movementPerTurn = defenderBase.movementPerTurn;
        moveInterpolationSpeed = defenderBase.moveInterpolationSpeed;
        moveInterpolationType = defenderBase.moveInterpolationType;
        attackRange = defenderBase.attackRange;
        attackChargeTime = defenderBase.attackChargeTime;
        heldWeaponPrefab = defenderBase.heldWeaponPrefab;
    }
}