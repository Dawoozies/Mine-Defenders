using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    float cooldown;
}
public interface IAbility
{
    //returns current cooldown
    //logic of which
    public float cooldown(float timeDelta);
}