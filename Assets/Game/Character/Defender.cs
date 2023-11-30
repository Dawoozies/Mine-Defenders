using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Defender : MonoBehaviour, IAgent, ITargeting
{
    [HideInInspector]
    public DefenderData defenderData;
    Vector3Int agentCellPos => GameManager.ins.WorldToCell(transform.position);
    Vector3 agentCellCenterPos => GameManager.ins.WorldToCellCenter(transform.position);
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;
    public Graphics baseGraphics;

    TargetingArgs ITargeting.args { get { return targetingData; } }
    TargetingArgs targetingData;
    public Vector3 worldPos => agentCellCenterPos;
    public Vector3Int cellPos => agentCellPos;
    public void Initialise(DefenderData defenderData)
    {
        this.defenderData = defenderData;
        SpriteAnimation defaultAnimation = new SpriteAnimation();
        defaultAnimation.animName = "Default";
        defaultAnimation.frames = 2;
        defaultAnimation.sprites = new List<Sprite>() { defenderData.defaultSprites[0], defenderData.defaultSprites[1] };
        baseGraphics.spriteAnimator.CreateAndPlayAnimation(defaultAnimation);

        agentData = new AgentArgs(transform, AgentType.Enemy, this);
        agentData.moveSpeed = defenderData.moveSpeed;
        agentData.notWalkable = GameManager.ins.GetEnemyInaccessibleTilemaps();
        agentData.reservedTiles = GameManager.ins.reservedTiles;
        agentData.moveInterpolationType = defenderData.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
        agentData.movesLeft = defenderData.moveSpeed;
        agentData.health = defenderData.health;
        agentData.allowedToLoot = LootType.None;

        targetingData = new TargetingArgs();
        targetingData.target = null;
        targetingData.targetedBy = new List<ITargeting>();
    }

    public Tilemap[] GetInaccessibleTilemaps()
    {
        return GameManager.ins.GetPlayerInaccessibleTilemaps();
    }
    public void UpdateTarget(List<ITargeting> potentialTargets)
    {
        if (potentialTargets == null || potentialTargets.Count == 0)
            return;
    }
    void ITargeting.SetTarget(ITargeting newTarget)
    {

    }
}