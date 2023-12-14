using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class AttackBase : ScriptableObject
{
    public Sprite icon;
    public int minRange;
    public int maxRange;
    public int damage;
    public float cooldown;
}

public interface IAttack
{
    public bool offCooldown { get; }
    public bool inRange(IAgent agent, IAgent target);
    public void startAttack(IAgent agent, IAgent target);
    public void cooldownUpdate(float timeDelta);
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
public class RangedAttack : IAttack
{
    public ProjectileAttackBase attackBase;
    public float time;
    public bool offCooldown { get { return time <= 0; } }
    public bool inRange(IAgent agent, IAgent target)
    {
        float distance = Vector3Int.Distance(agent.args.cellPos, target.args.cellPos);
        if (distance < attackBase.minRange) 
            return false;
        if(distance > attackBase.maxRange) 
            return false;
        return true;
    }

    public void startAttack(IAgent agent, IAgent target)
    {
        //set up animation stuff
        //do event subscription stuff
    }
    public void cooldownUpdate(float timeDelta) 
    {
        if (time > 0)
            time -= timeDelta;
    }
}
public enum AttackType
{
}
public static class AttackFactory
{

}