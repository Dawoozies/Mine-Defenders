using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IAgent
{
    EnemyBase enemyBase;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;

    List<Attack> onCooldown = new();
    List<Attack> available = new();
    public Graphics baseGraphics;
    float attackCharge;
    public Vector3 worldPos => agentCellCenterPos;
    public Vector3Int cellPos => agentCellPos;
    UI_Action_Display actionDisplay;
    public void Initialise(EnemyBase enemyBase)
    {
        this.enemyBase = enemyBase;
        #region Alive Animation
        SpriteAnimation aliveSpriteAnimation = new SpriteAnimation();
        aliveSpriteAnimation.animName = "Alive";
        aliveSpriteAnimation.frames = 2;
        aliveSpriteAnimation.sprites = new List<Sprite> { enemyBase.defaultSprites[0], enemyBase.defaultSprites[1] };
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
        deadSpriteAnimation.sprites = new List<Sprite> { enemyBase.defaultSprites[0], enemyBase.defaultSprites[0] };
        deadSpriteAnimation.spriteColors = new List<Color> { Color.white * 0.5f, Color.white * 0.5f };
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

        foreach (AttackBase attackBase in enemyBase.attackBases)
        {
            Attack attack = new Attack(attackBase);
            available.Add(attack);
        }

        //Set up agent data
        agentData = new AgentArgs(transform, AgentType.Enemy, this);
        agentData.moveSpeed = enemyBase.moveSpeed;
        agentData.notWalkable = GameManager.ins.GetEnemyInaccessibleTilemaps();
        agentData.reservedTiles = GameManager.ins.reservedTiles;
        agentData.moveInterpolationType = enemyBase.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
        agentData.movesLeft = enemyBase.moveSpeed;
        agentData.health = enemyBase.baseHealth;
        agentData.allowedToLoot = LootType.None;
        agentData.target = null;
        agentData.targetedBy = new List<IAgent>();

        agentData.onDeath += () => { 
            if(actionDisplay != null)
            {
                actionDisplay.ReturnToPool();
                actionDisplay = null;
            }
            baseGraphics.spriteAnimator.PlayAnimation("Dead");
            baseGraphics.objectAnimator.PlayAnimation("Dead");
        };
    }
    void Update()
    {
        if (agentData.isDead)
            return;
        if (!agentData.isActive)
            return;
        attackCharge += Time.deltaTime;
        //bool canAttack = GameManager.ins.DistanceFromPlayer(this) <= 1 && available.Count > 0;
        if (agentData.target == null)
            return;
        bool canAttack = available.Count > 0 && Vector3Int.Distance(cellPos, agentData.target.args.cellPos) <= 1;
        if(attackCharge >= enemyBase.attackChargeTime && canAttack)
        {
            StartAttackAnimation();
            attackCharge = 0;
        }
        Cooldown();
    }
    void Cooldown()
    {
        List<Attack> onCooldownNew = new List<Attack>();
        foreach (Attack attack in onCooldown)
        {
            attack.CooldownUpdate(Time.deltaTime);
            if(!attack.offCooldown)
            {
                onCooldownNew.Add(attack);
            }
            else
            {
                available.Add(attack);
            }
        }
        onCooldown = onCooldownNew;
    }
    Attack GetAttack()
    {
        int selectedIndex = Random.Range(0, available.Count);
        Attack selectedAttack = available[selectedIndex];
        onCooldown.Add(available[selectedIndex]);
        available.RemoveAt(selectedIndex);
        return selectedAttack;
    }
    void StartAttackAnimation()
    {
        Attack selectedAttack = GetAttack();
        ObjectAnimation attackAnimation = new ObjectAnimation();
        attackAnimation.animName = "Attack";
        attackAnimation.frames = 3;
        //Vector3 dirToPlayer = Vector3.Normalize(GameManager.ins.GetPlayerWorldPosition() - agentCellCenterPos);
        Vector3 dirToTarget = Vector3.Normalize(agentData.target.args.worldPos - agentCellCenterPos);
        attackAnimation.positions = new List<Vector3>
        {
            Vector3.zero,
            dirToTarget*0.5f,
            Vector3.zero,
        };
        attackAnimation.interpolationTypes = new List<InterpolationType>
        {
            selectedAttack.attackBase.interpolationType,
            selectedAttack.attackBase.interpolationType,
            selectedAttack.attackBase.interpolationType,
        };
        baseGraphics.objectAnimator.animationSpeed = selectedAttack.attackBase.interpolationSpeed;
        baseGraphics.objectAnimator.CreateAndPlayAnimation(attackAnimation);
        //do ui action display
        actionDisplay = UIManager.ins.Get_Action_Display().TrackingRequest(
            this, 
            Vector3.zero,
            selectedAttack.attackBase.icon
            );
        baseGraphics.objectAnimator.onAnimationComplete += () => {
            if (actionDisplay != null)
            {
                actionDisplay.ReturnToPool();
                actionDisplay = null;
            }
        };
        baseGraphics.objectAnimator.onAnimationComplete += () => {
            if (agentData.target == null)
                return;
            agentData.target.args.AttackAgent(this, selectedAttack);
        };
    }
    public Tilemap[] GetInaccessibleTilemaps()
    {
        return GameManager.ins.GetEnemyInaccessibleTilemaps();
    }
    public void Retarget()
    {
        if (agentData.isDead)
            return;
        if (!agentData.isActive)
            return;
        if (agentData.target != null)
            return;
        List<Defender> defenders = GameManager.ins.GetDefenders();
        foreach (IAgent defender in defenders)
        {
            if (defender.args.isDead)
                continue;
            if (!defender.args.isActive)
                continue;
            if (defender.args.targetedBy.Count >= 4)
                continue;
            defender.args.targetedBy.Add(this);
            agentData.target = defender;
            return;
        }
    }
    //public IAgent GetNewTarget_Enemy(Enemy enemy)
    //{
    //    if (((IAgent)enemy).args.target != null)
    //    {
    //        //Don't try to swap target if we have already targeted a defender
    //        if (agentController.activeDefenders.Contains(((IAgent)enemy).args.target as Defender))
    //            return ((IAgent)enemy).args.target;
    //        ((IAgent)enemy).args.target.args.targetedBy.Remove(enemy);
    //    }
    //    for (int i = 0; i < agentController.activeDefenders.Count; i++)
    //    {
    //        IAgent defender = agentController.activeDefenders[i];
    //        if (defender.args.isDead)
    //            continue;
    //        if (defender.args.targetedBy.Count >= 4)
    //            continue;
    //        if (!agentController.CanPathToTarget(enemy, defender))
    //            continue;
    //        int freeSpaces = agentController.AvailableAdjacentSpaces(enemy, defender);
    //        if (defender.args.targetedBy.Count >= freeSpaces)
    //            continue;

    //        defender.args.targetedBy.Add(enemy);
    //        return defender;
    //    }
    //    IAgent player = agentController.player;
    //    if (!player.args.isDead && agentController.CanPathToTarget(enemy, player))
    //    {
    //        return player;
    //    }
    //    return null;
    //}
}