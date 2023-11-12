using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class AttackBase : ScriptableObject
{
    public Sprite icon;
    public int damage;
    public int energyCost;
    public float cooldown;
    public float hitChance;
    public float interpolationSpeed;
    public InterpolationType interpolationType;
    //{agentName} {logUseText} {targetAgentName}
    public string logUseText;
    public string logSucceedText;
    public string logFailText;
}
public class Attack
{
    public AttackBase attackBase;
    public float time;
    public bool offCooldown { get { return time <= 0; } }
    public Attack(AttackBase attackBase)
    {
        this.attackBase = attackBase;
        time = 0;
    }
    public void CooldownUpdate(float timeDelta)
    {
        if(time > 0)
        {
            time -= timeDelta;
        }
    }
}
