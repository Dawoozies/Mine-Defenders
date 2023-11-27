using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour, IAgent
{
    [HideInInspector]
    public DefenderData defenderData;
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;
    public Graphics baseGraphics;
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
    }
}