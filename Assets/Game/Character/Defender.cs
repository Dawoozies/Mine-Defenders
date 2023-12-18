using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
public class Defender : MonoBehaviour, IAgent
{
    [HideInInspector]
    public DefenderData defenderData;
    Vector3Int agentCellPos => GameManager.ins.WorldToCell(transform.position);
    Vector3 agentCellCenterPos => GameManager.ins.WorldToCellCenter(transform.position);
    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;
    //List<Attack> onCooldown = new();
    //List<Attack> available = new();
    float attackCharge;
    public Graphics baseGraphics;
    public Vector3 worldPos => agentCellCenterPos;
    public Vector3Int cellPos => agentCellPos;
    UI_Action_Display actionDisplay;
    List<AgentType> targetTypes = new List<AgentType> { AgentType.Enemy };

    IHeldWeapon heldWeapon;
    public void Initialise(DefenderData defenderData)
    {
        this.defenderData = defenderData;
        #region Alive Animation
        SpriteAnimation aliveSpriteAnimation = new SpriteAnimation();
        aliveSpriteAnimation.animName = "Alive";
        aliveSpriteAnimation.frames = 2;
        aliveSpriteAnimation.sprites = new List<Sprite> { defenderData.defaultSprites[0], defenderData.defaultSprites[1] };
        aliveSpriteAnimation.spriteColors = new List<Color> { Color.white, Color.white };
        aliveSpriteAnimation.spriteOrders = new List<int> { 4, 4};
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
        deadSpriteAnimation.spriteOrders = new List<int> { 3, 3 };
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
        //foreach (AttackBase attackBase in defenderData.attackBases)
        //{
        //    Attack attack = new Attack(attackBase);
        //    available.Add(attack);
        //}
        agentData = new AgentArgs(transform, AgentType.Enemy, this);
        agentData.movementPerTurn = defenderData.movementPerTurn;
        agentData.moveInterpolationSpeed = defenderData.moveInterpolationSpeed;
        agentData.moveInterpolationType = defenderData.moveInterpolationType;
        agentData.previousPoint = new Vector3Int(0, 0, -1);
        agentData.movesLeft = 0;
        agentData.health = defenderData.health;
        agentData.exp = defenderData.exp;
        agentData.allowedToLoot = LootType.None;
        agentData.target = null;
        agentData.targetedBy = new List<IAgent>();

        agentData.onDeath += () => {
            if (actionDisplay != null)
            {
                actionDisplay.ReturnToPool();
                actionDisplay = null;
            }
            baseGraphics.spriteAnimator.PlayAnimation("Dead");
            baseGraphics.objectAnimator.PlayAnimation("Dead");
        };

        //agentData.attackRange = defenderData.attackRange;
        //agentData.attacks = new List<Attack>();
        //foreach (AttackBase attackBase in defenderData.attackBases)
        //{
        //    agentData.attacks.Add(new Attack(attackBase));
        //}
        if(defenderData.heldWeaponPrefab != null)
        {
            GameObject heldWeaponClone = Instantiate(defenderData.heldWeaponPrefab, transform);
            heldWeapon = heldWeaponClone.GetComponent<IHeldWeapon>();
            heldWeapon.owner = this;
        }
    }
    void Update()
    {
        if (agentData.isDead) return;
        if (!agentData.isActive) return;
        if(agentData.target != null)
        {
            attackCharge += Time.deltaTime;
            bool attackCharged = attackCharge >= defenderData.attackChargeTime;
            if (heldWeapon != null)
            {
                heldWeapon.target = agentData.target;
                (HeldWeaponState, bool) stateStatus = heldWeapon.StateStatus();
                heldWeapon.dirToTarget = (agentData.target.args.worldPos - transform.position).normalized;
                if(attackCharged)
                    heldWeapon.SetWeaponState(HeldWeaponState.ReadyingAttack, 1f);
                if (stateStatus.Item1 == HeldWeaponState.ReadyingAttack && stateStatus.Item2)
                    heldWeapon.SetWeaponState(HeldWeaponState.Attacking, 2f); //fire off attack
            }
            if (attackCharge >= defenderData.attackChargeTime)
                attackCharge = 0;
        }
    }
    //void Update()
    //{
    //    if (agentData.isDead)
    //        return;
    //    if (!agentData.isActive)
    //        return;
    //    attackCharge += Time.deltaTime;
    //    if (agentData.target == null)
    //        return;
    //    bool canAttack = available.Count > 0 && Vector3Int.Distance(cellPos, agentData.target.args.cellPos) <= 1;
    //    if(attackCharge >= defenderData.attackChargeTime && canAttack)
    //    {
    //        //StartAttackAnimation();
    //        attackCharge = 0;
    //    }
    //    Cooldown();
    //}
    //void Cooldown()
    //{
    //    List<Attack> onCooldownNew = new List<Attack>();
    //    foreach (Attack attack in onCooldown)
    //    {
    //        attack.CooldownUpdate(Time.deltaTime);
    //        if (!attack.offCooldown)
    //            onCooldownNew.Add(attack);
    //        else
    //            available.Add(attack);
    //    }
    //    onCooldown = onCooldownNew;
    //}
    //Attack GetAttack()
    //{
    //    int selectedIndex = Random.Range(0, available.Count);
    //    Attack selectedAttack = available[selectedIndex];
    //    onCooldown.Add(available[selectedIndex]);
    //    available.RemoveAt(selectedIndex);
    //    return selectedAttack;
    //}
    //void StartAttackAnimation()
    //{
    //    Attack selectedAttack = GetAttack();
    //    ObjectAnimation attackAnimation = new ObjectAnimation();
    //    attackAnimation.animName = "Attack";
    //    attackAnimation.frames = 3;
    //    Vector3 dirToTarget = Vector3.Normalize(agentData.target.args.worldPos - agentCellCenterPos);
    //    attackAnimation.positions = new List<Vector3>
    //    {
    //        Vector3.zero,
    //        dirToTarget*0.5f,
    //        Vector3.zero,
    //    };
    //    attackAnimation.interpolationTypes = new List<InterpolationType>
    //    {
    //        selectedAttack.attackBase.interpolationType,
    //        selectedAttack.attackBase.interpolationType,
    //        selectedAttack.attackBase.interpolationType,
    //    };
    //    baseGraphics.objectAnimator.animationSpeed = selectedAttack.attackBase.interpolationSpeed;
    //    baseGraphics.objectAnimator.CreateAndPlayAnimation(attackAnimation);
    //    //do ui action display
    //    if(actionDisplay == null)
    //    {
    //        actionDisplay = UIManager.ins.Get_Action_Display().TrackingRequest(
    //            this,
    //            Vector3.zero,
    //            selectedAttack.attackBase.icon
    //            );
    //    }
    //    baseGraphics.objectAnimator.onAnimationComplete += () => {
    //        if (actionDisplay != null)
    //        {
    //            actionDisplay.ReturnToPool();
    //            actionDisplay = null;
    //        }
    //    };
    //    baseGraphics.objectAnimator.onAnimationComplete += () => {
    //        if (agentData.target == null)
    //            return;
    //        agentData.target.args.AttackAgent(this, selectedAttack);
    //    };
    //}
    public Tilemap[] GetInaccessibleTilemaps()
    {
        return GameManager.ins.GetPlayerInaccessibleTilemaps();
    }
    public virtual void Retarget()
    {
        if (agentData.isDead)
            return;
        if (!agentData.isActive)
            return;
        if (agentData.target != null)
            return;
        List<Enemy> enemies = GameManager.ins.GetEnemies();
        List<IAgent> targetable = new List<IAgent>();
        foreach (IAgent enemy in enemies)
        {
            if (enemy.args.isDead)
                continue;
            if (!enemy.args.isActive)
                continue;
            targetable.Add(enemy);
        }
        if (targetable == null || targetable.Count == 0)
            return;
        IAgent closestEnemy = targetable[0];
        foreach (IAgent enemy in targetable)
        {
            if (Vector3Int.Distance(agentData.cellPos, enemy.args.cellPos) < Vector3Int.Distance(agentData.cellPos, closestEnemy.args.cellPos))
            {
                closestEnemy = enemy;
                continue;
            }
        }
        agentData.SetTarget(closestEnemy);
    }
}