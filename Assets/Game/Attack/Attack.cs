using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class Attack : ScriptableObject
{
    public Sprite icon;
    public int damage;
    public int energyCost;
    public float cooldown;
    public float hitChance;
    //{agentName} {logUseText} {targetAgentName}
    public string logUseText;
    public string logSucceedText;
    public string logFailText;
}
