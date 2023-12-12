using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//this schematic should be a json structure
//Schematic for building an ability
public class AbilityBase : ScriptableObject
{
    public Sprite icon;
    public int range;
    public float cooldownTime;
}
//its just like the input event system
//on performed
//on completed
//on cancel
public class AbilityData
{
    public AbilityBase abilityBase;
    public float cooldownTime = 0;
    public virtual float cooldown(float timeDelta)
    {
        if (cooldownTime > 0)
            cooldownTime -= timeDelta;
        return cooldownTime;
    }
}
public interface IAbility
{
    //returns current cooldown
    //logic of which
    public float cooldown(float timeDelta);
    //overwrite with ability specific requirements
    public bool rangeRequirement(IAgent agent, IAgent target);
}
//Melee attacks
//how to apply a buff/debuff?
//buffs and debuffs will have to be scriptable objects themselves which give info
//buff gets put in a list and processed by the affected agent on update
class MeleeAttack : AbilityData, IAbility
{
    public bool rangeRequirement(IAgent agent, IAgent target)
    {
        return Vector3Int.Distance(agent.args.cellPos, target.args.cellPos) <= abilityBase.range;
    }
}