using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class AttackBase : ScriptableObject, IAttack
{
    public Sprite icon;
    public int minRange;
    public int maxRange;
    public int damage;
    public float cooldown;
    public float interpolationSpeed;
    public InterpolationType interpolationType;
    public virtual float ComputeHeuristic(IAgent agent, IAgent potentialTarget)
    {
        float distanceToTarget = Vector3Int.Distance(agent.args.cellPos, potentialTarget.args.cellPos);
        if (distanceToTarget < minRange)
            return -1;
        if (distanceToTarget > maxRange)
            return -1;
        float damagePerCooldown = damage / cooldown;
        float rangeLength = maxRange - minRange;
        float effectiveDamageArea = damagePerCooldown * rangeLength;
        return effectiveDamageArea;
    }
}
public interface IAttack
{
    public float ComputeHeuristic(IAgent agent, IAgent potentialTarget);
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