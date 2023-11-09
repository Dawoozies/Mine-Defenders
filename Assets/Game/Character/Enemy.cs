using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IAgent
{
    NavigationOrder order;
    EnemyBase enemyBase;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }
    AgentType IAgent.AgentType {
        get => AgentType.Enemy;
    }
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;

    AgentNavigator IAgent.navigator { get { return agentNavigator; } }
    AgentNavigator agentNavigator;


    [Serializable]
    public class Graphics
    {
        public SpriteAnimator spriteAnimator;
        public ObjectAnimator objectAnimator;
    }
    public Graphics baseGraphics;
    public void Initialise(EnemyBase enemyBase)
    {
        this.enemyBase = enemyBase;
        //Set up sprite stuff
        SpriteAnimation defaultAnimation = new SpriteAnimation();
        defaultAnimation.animName = "Default";
        defaultAnimation.frames = 2;
        defaultAnimation.sprites = new List<Sprite>() { enemyBase.defaultSprites[0], enemyBase.defaultSprites[1] };
        baseGraphics.spriteAnimator.CreateAndPlayAnimation(defaultAnimation);

        //Set up agent data
        agentData = new AgentArgs(transform);
        agentData.moveSpeed = enemyBase.moveSpeed;
        agentData.notWalkable = GameManager.ins.GetEnemyInaccessibleTilemaps();
        agentData.reservedTiles = GameManager.ins.reservedTiles;
        agentData.moveInterpolationType = enemyBase.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
    }
}