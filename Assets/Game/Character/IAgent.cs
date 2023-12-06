using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public interface IAgent
{
    public AgentArgs args { get; }
    public Tilemap[] GetInaccessibleTilemaps();
    public void Retarget();
}
public enum AgentType
{
    Player = 0,
    Enemy = 1,
    Defender = 2,
}
public class AgentArgs
{
    IAgent agent;
    public AgentType type;
    public Transform transform { get; set; }
    public int movementPerTurn;
    public Vector3Int cellPos => GameManager.ins.WorldToCell(transform.position);
    public Vector3 worldPos => GameManager.ins.WorldToCellCenter(transform.position);
    public Vector3 screenPos => ScreenTrackingWithOffset(Vector3.zero);
    public float moveInterpolationSpeed;
    public InterpolationType moveInterpolationType;
    public AgentPath path;
    public Vector3Int previousPoint;
    public bool hasInstruction;
    public int movesLeft;

    public List<AgentPath> playerPath;
    public int playerPathIndex;
    public bool pathAtIndexCompleted;
    public bool finalPathCompleted;
    public delegate void PlayerNoMovesLeft();
    public event PlayerNoMovesLeft onPlayerNoMovesLeft;
    public delegate void PlayerCompletedFullPath();
    public event PlayerCompletedFullPath onPlayerCompletedFullPath;

    public int health;
    public int exp;
    public LootType allowedToLoot;
    public Dictionary<string, int> lootDictionary;

    public IAgent target;
    public List<IAgent> targetedBy;
    public bool isDead;
    public bool isActive;

    public delegate void OnDeath();
    public event OnDeath onDeath;
    public AgentArgs(Transform transform, AgentType type, IAgent agent)
    {
        this.transform = transform;
        this.type = type;
        this.agent = agent;
        lootDictionary = new Dictionary<string, int>();
    }
    public void AgentDeath()
    {
        if(!isDead)
        {
            isDead = true;
            if(target != null)
            {
                target.args.targetedBy.Remove(agent);
                target = null;
            }
            if (targetedBy == null || targetedBy.Count == 0)
                return;
            while (targetedBy.Count > 0)
            {
                targetedBy[0].args.target = null;
                targetedBy[0].Retarget();
                targetedBy.RemoveAt(0);
            }
            onDeath?.Invoke();
        }
    }
    public void AttackAgent(IAgent attackingAgent, Attack attackUsed)
    {
        if(health > 0)
        {
            health -= attackUsed.attackBase.damage;
            if (health <= 0)
            {
                health = 0;
                AgentDeath();
                return;
            }
            if (target == null)
            {
                target = attackingAgent;
                attackingAgent.args.targetedBy.Add(agent);
            }
            else
            {
                if(target != attackingAgent)
                {
                    float currentTargetDistance = Vector3Int.Distance(cellPos, target.args.cellPos);
                    float attackingAgentDistance = Vector3Int.Distance(cellPos, attackingAgent.args.cellPos);
                    if(attackingAgentDistance < currentTargetDistance)
                    {
                        target.args.targetedBy.Remove(agent);
                        target = attackingAgent;
                        attackingAgent.args.targetedBy.Add(agent);
                    }
                }
            }
        }
    }
    public void PickupLoot(CellLoot loot)
    {
        if(!lootDictionary.ContainsKey(loot.lootName))
        {
            lootDictionary.Add(loot.lootName, loot.amount);
            Debug.LogWarning($"{type} picking up {loot.lootName}. Amount in backpack going from 0 to {lootDictionary[loot.lootName]}. Increase by {loot.amount}");
            return;
        }
        Debug.LogWarning($"{type} picking up {loot.lootName}. Amount in backpack going from {lootDictionary[loot.lootName]} to {lootDictionary[loot.lootName]+loot.amount}. Increase by {loot.amount}");
        lootDictionary[loot.lootName] = lootDictionary[loot.lootName] + loot.amount;
    }
    public void MoveAlongPath(float timeDelta)
    {
        if(type == AgentType.Enemy || type == AgentType.Defender)
        {
            if (path == null || movesLeft <= 0)
                return;
            //Debug.Log($"Moving along path {path.start} -> {path.end}");
            if (path.completed)
            {
                //Debug.Log("path part completed");
                GameManager.ins.TryLootAtCell(path.end, agent);
                previousPoint = path.start;
                if (hasInstruction)
                    hasInstruction = false;
                movesLeft--;
                return;
            }
            transform.position = path.Traverse(timeDelta * moveInterpolationSpeed);
        }
        if(type == AgentType.Player)
        {
            if(movesLeft <= 0)
            {
                onPlayerNoMovesLeft?.Invoke();
                movesLeft = movementPerTurn;
            }
            if (playerPath == null || playerPath.Count == 0)
                return;
            if (playerPathIndex >= playerPath.Count)
            {
                //Debug.Log("player path is complete");
                onPlayerCompletedFullPath?.Invoke();
                playerPath = null;
                return;
            }
            if (playerPath[playerPathIndex].completed)
            {
                GameManager.ins.TryLootAtCell(playerPath[playerPathIndex].end, agent);
                if (playerPathIndex + 1 <= playerPath.Count)
                {
                    //Debug.Log($"player path at index {playerPathIndex} is complete and there is another");
                    //this makes the player move from current path to next one
                    movesLeft--;
                    playerPathIndex++;
                    return;
                    //Debug.Log($"Increase path index to {playerPathIndex}");
                }
                else
                {
                    if (!finalPathCompleted)
                    {
                        Debug.Log("This is the last path index");
                        finalPathCompleted = true;
                    }
                }
            }
            transform.position = playerPath[playerPathIndex].Traverse(timeDelta * moveInterpolationSpeed);
        }
    }
    public void RefreshMovesLeft()
    {
        Debug.Log($"Refreshing movesLeft for {transform.name}");
        movesLeft = movementPerTurn;
    }
    public void ResetCompletedFullPathEvent()
    {
        onPlayerCompletedFullPath = null;
    }
    public Vector3 ScreenTrackingWithOffset(Vector3 offset)
    {
        return GameManager.ins.WorldToScreenPosition(transform.position + offset);
    }
    public void SetTarget(IAgent newTarget)
    {
        if(target != null)
            target.args.targetedBy.Remove(agent);

        target = newTarget;
        newTarget.args.targetedBy.Add(agent);
    }
}
public class AgentPath
{
    public Vector3Int start;
    public Vector3Int end;
    public InterpolationType interpolationType;
    public float t;
    public bool completed { get { return t >= 1; } }
    public AgentPath(Vector3Int start, Vector3Int end, InterpolationType interpolationType)
    {
        this.start = start;
        this.end = end;
        this.interpolationType = interpolationType;
        t = 0;
    }
    public Vector3 Traverse(float timeDelta)
    {
        var p = Interpolation.Interpolate(start, end, t, interpolationType);
        t += timeDelta;
        return p;
    }
}
[Serializable]
public class Graphics
{
    public SpriteAnimator spriteAnimator;
    public ObjectAnimator objectAnimator;
}