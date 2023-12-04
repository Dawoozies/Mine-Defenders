using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Defender : MonoBehaviour, IAgent
{
    [HideInInspector]
    public DefenderData defenderData;
    Vector3Int agentCellPos => GameManager.ins.WorldToCell(transform.position);
    Vector3 agentCellCenterPos => GameManager.ins.WorldToCellCenter(transform.position);
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;
    public Graphics baseGraphics;
    public Vector3 worldPos => agentCellCenterPos;
    public Vector3Int cellPos => agentCellPos;
    public void Initialise(DefenderData defenderData)
    {
        this.defenderData = defenderData;
        #region Alive Animation
        SpriteAnimation aliveSpriteAnimation = new SpriteAnimation();
        aliveSpriteAnimation.animName = "Alive";
        aliveSpriteAnimation.frames = 2;
        aliveSpriteAnimation.sprites = new List<Sprite> { defenderData.defaultSprites[0], defenderData.defaultSprites[1] };
        aliveSpriteAnimation.spriteColors = new List<Color> { Color.white, Color.white };
        baseGraphics.spriteAnimator.CreateAndPlayAnimation(aliveSpriteAnimation);
        ObjectAnimation aliveObjectAnimation = new ObjectAnimation();
        aliveObjectAnimation.animName = "Alive";
        aliveObjectAnimation.frames = 2;
        aliveObjectAnimation.rotations = new List<Quaternion> { 
            Quaternion.identity, 
            Quaternion.identity 
        };
        aliveObjectAnimation.interpolationTypes = new List<InterpolationType> {
            InterpolationType.Linear,
            InterpolationType.Linear,
        };
        baseGraphics.objectAnimator.CreateAndPlayAnimation(aliveObjectAnimation);
        #endregion
        #region Dead Animation
        SpriteAnimation deadSpriteAnimation = new SpriteAnimation();
        deadSpriteAnimation.animName = "Dead";
        deadSpriteAnimation.frames = 2;
        deadSpriteAnimation.sprites = new List<Sprite>{ defenderData.defaultSprites[0], defenderData.defaultSprites[0] };
        deadSpriteAnimation.spriteColors = new List<Color>{ Color.white*0.5f, Color.white*0.5f };
        baseGraphics.spriteAnimator.CreateAnimation(deadSpriteAnimation);
        ObjectAnimation deadObjectAnimation = new ObjectAnimation();
        deadObjectAnimation.animName = "Dead";
        deadObjectAnimation.frames = 2;
        deadObjectAnimation.rotations = new List<Quaternion>{
            Quaternion.identity,
            Quaternion.AngleAxis(90, Vector3.forward),
        };
        deadObjectAnimation.interpolationTypes = new List<InterpolationType>{ 
            InterpolationType.EaseOutElastic,
            InterpolationType.EaseOutElastic,
        };
        baseGraphics.objectAnimator.CreateAnimation(deadObjectAnimation);
        #endregion
        agentData = new AgentArgs(transform, AgentType.Enemy, this);
        agentData.moveSpeed = defenderData.moveSpeed;
        agentData.notWalkable = GameManager.ins.GetEnemyInaccessibleTilemaps();
        agentData.reservedTiles = GameManager.ins.reservedTiles;
        agentData.moveInterpolationType = defenderData.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
        agentData.movesLeft = defenderData.moveSpeed;
        agentData.health = defenderData.health;
        agentData.allowedToLoot = LootType.None;
        agentData.target = null;
        agentData.targetedBy = new List<IAgent>();
    }
    public Tilemap[] GetInaccessibleTilemaps()
    {
        return GameManager.ins.GetPlayerInaccessibleTilemaps();
    }
    public void DamageAgent(Attack attack)
    {
        if(defenderData.health > 0)
            defenderData.health -= attack.attackBase.damage;

        if(defenderData.health <= 0)
        {
            defenderData.health = 0;
            baseGraphics.spriteAnimator.PlayAnimation("Dead");
            baseGraphics.objectAnimator.PlayAnimation("Dead");
            agentData.AgentDeath();
        }
        agentData.health = defenderData.health;
    }
    public bool HasValidTarget()
    {
        if (agentData.target == null)
            return false;
        if(agentData.target.args.isDead)
        {
            agentData.target = null;
            return false;
        }
        return true;
    }
    public void Retarget()
    {

    }
}