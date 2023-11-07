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
    }
    void IAgent.ReserveCell(CellData cellData)
    {
        throw new System.NotImplementedException();
    }

    void IAgent.SetMovementOrder(CellData cellData)
    {
        throw new System.NotImplementedException();
    }
}