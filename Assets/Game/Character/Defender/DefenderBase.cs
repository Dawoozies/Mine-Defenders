using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class DefenderBase : ScriptableObject
{
    public int baseHealth;
    public Sprite[] defaultSprites;
    public AttackBase[] attackBases;
}
[Serializable]
public class Defender
{
    public string defenderName;
    public int defenderKey;
    public int maxHealth, health;
    public int maxExp, exp;
    public Sprite[] defaultSprites;
    public AttackBase[] attackBases;
    public Defender(DefenderJSON defenderJSON, DefenderBase defenderBase)
    {
        defenderName = defenderJSON.defenderBaseName;
        defenderKey = defenderJSON.defenderKey;
        maxHealth = defenderJSON.maxHealth;
        health = defenderJSON.health;
        maxExp = defenderJSON.maxExp;
        exp = defenderJSON.exp;
        defaultSprites = defenderBase.defaultSprites;
        attackBases = defenderBase.attackBases;
    }
}