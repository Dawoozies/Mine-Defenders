using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IAgent
{
    NavigationOrder order;
    EnemyBase enemyBase;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;

    List<Attack> onCooldown = new();
    List<Attack> available = new();
    [Serializable]
    public class Graphics
    {
        public SpriteAnimator spriteAnimator;
        public ObjectAnimator objectAnimator;
    }
    public Graphics baseGraphics;
    float attackCharge;
    public void Initialise(EnemyBase enemyBase)
    {
        this.enemyBase = enemyBase;
        //Set up sprite stuff
        SpriteAnimation defaultAnimation = new SpriteAnimation();
        defaultAnimation.animName = "Default";
        defaultAnimation.frames = 2;
        defaultAnimation.sprites = new List<Sprite>() { enemyBase.defaultSprites[0], enemyBase.defaultSprites[1] };
        baseGraphics.spriteAnimator.CreateAndPlayAnimation(defaultAnimation);

        foreach (AttackBase attackBase in enemyBase.attackBases)
        {
            Attack attack = new Attack(attackBase);
            available.Add(attack);
        }

        //Set up agent data
        agentData = new AgentArgs(transform, AgentType.Enemy);
        agentData.moveSpeed = enemyBase.moveSpeed;
        agentData.notWalkable = GameManager.ins.GetEnemyInaccessibleTilemaps();
        agentData.reservedTiles = GameManager.ins.reservedTiles;
        agentData.moveInterpolationType = enemyBase.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
        agentData.movesLeft = enemyBase.moveSpeed;
        agentData.health = enemyBase.hearts * 4;
    }
    void Update()
    {
        attackCharge += Time.deltaTime;
        bool canAttack = GameManager.ins.DistanceFromPlayer(this) <= 1 && available.Count > 0;
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
        Vector3 dirToPlayer = Vector3.Normalize(GameManager.ins.GetPlayerWorldPosition() - agentCellCenterPos);
        attackAnimation.positions = new List<Vector3>
        {
            Vector3.zero,
            dirToPlayer*0.5f,
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
        UI_Action_Display actionDisplay = UIManager.ins.Get_Action_Display().TrackingRequest(
            this, 
            Vector3.zero,
            selectedAttack.attackBase.icon
            );
        baseGraphics.objectAnimator.onAnimationComplete += actionDisplay.ReturnToPool;
    }
}