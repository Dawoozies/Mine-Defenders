using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour, IAgent
{
    NavigationOrder order;
    public float movementSpeed;
    public InterpolationType moveInterpolationType;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }
    AgentType IAgent.AgentType { 
        get => AgentType.Player; 
    }
    Buffer<Vector3> agentPositionBuffer;
    [Serializable]
    public class Tool
    {
        [HideInInspector]
        public Vector3Int toolTargetCellPos;
        [HideInInspector]
        public Vector3 toolTargetCellCenterWorldPos;
        public int baseDamage;
        public SpriteAnimator spriteAnimator;
        public ObjectAnimator objectAnimator;
    }
    public Tool tool;
    private void Awake()
    {
        GameManager.onTap += InteractWithCell;

        agentPositionBuffer = new Buffer<Vector3>(agentCellCenterPos, 0.125f);
        agentPositionBuffer.onWriteToBuffer += () => { GameManager.OnPlayerPositionUpdated += agentPositionBuffer.GetBuffer; };

        tool.objectAnimator.TimeUpdateEvent += (float time, int index, string animName) => {
            if (animName != "Mining")
                return;
            if(index == 1)
            {
                //Damage durability of cell at tool target position
                CellData refToCell = GameManager.ins.GetCellDataAtPosition(tool.toolTargetCellPos);
                refToCell.durability -= tool.baseDamage;
                if (refToCell.durability <= 0)
                {
                    GameManager.ins.BreakStone(refToCell);
                    tool.objectAnimator.PlayAnimation("NotMining");
                    tool.spriteAnimator.PlayAnimation("PutAway");
                }
            }
        };
    }
    void InteractWithCell(CellData tappedCell)
    {
        //tappedCell.CellInfoDebug();
        //if no durability move
        bool hasDurability = tappedCell.durability > 0;
        if(hasDurability)
            SetMiningOrder(tappedCell);
        if (!hasDurability)
            SetMovementOrder(tappedCell);
    }
    public void SetMovementOrder(CellData cellData)
    {
        order = new NavigationOrder(
            "MoveToCell",
            agentCellPos,
            cellData.cellPosition,
            GameManager.ins.GetPlayerInaccessibleTilemaps(),
            false,
            0,
            moveInterpolationType
            );
    }
    public void SetMiningOrder(CellData cellData)
    {
        tool.toolTargetCellPos = cellData.cellPosition;
        tool.toolTargetCellCenterWorldPos = cellData.cellCenterWorldPosition;
        List<Vector3Int> shortestPathToNeighbour
            = cellData.GetPathToClosestCardinalNeighbour(agentCellPos, GameManager.ins.GetPlayerInaccessibleTilemaps(), null);
        //If we are right next to what we want to mine then just start mining animation
        if(Vector3Int.Distance(agentCellPos, cellData.cellPosition) == 1)
        {
            StartMiningAnimation();
        }
        //Else we must use the code below
        if (shortestPathToNeighbour == null)
            return;
        order = new NavigationOrder(
            "MoveToMiningTarget",
            shortestPathToNeighbour,
            GameManager.ins.GetPlayerInaccessibleTilemaps(),
            false,
            0, //No cutoff since we are already pathing to an adjacent point
            moveInterpolationType
            );
        order.onNavigationComplete += StartMiningAnimation;
    }
    void StartMiningAnimation()
    {
        tool.spriteAnimator.PlayAnimation("Pickaxe_Default");
        ObjectAnimation miningAnimation = new ObjectAnimation();
        miningAnimation.animName = "Mining";
        miningAnimation.frames = 3;
        Vector3 dirToStone = Vector3.Normalize(tool.toolTargetCellCenterWorldPos - agentCellCenterPos);
        miningAnimation.positions = new List<Vector3> {
            Vector3.zero,
            -dirToStone*0.5f,
            dirToStone*0.65f,
        };
        Quaternion RotBy90 = Quaternion.AngleAxis(90, Vector3.forward);
        Quaternion ReflectBy180 = Quaternion.AngleAxis(180, Vector3.up);
        bool horizontalReflectCondition = Mathf.Min(0, Mathf.Sign(Vector3.Dot(-dirToStone, Vector3.right))) < 0;
        bool verticalReflectCondition = Mathf.Min(0, Mathf.Sign(Vector3.Dot(dirToStone, Vector3.up))) < 0;
        Quaternion readyRotation = QuaternionUtility.Conditional(
            ReflectBy180,
            Quaternion.FromToRotation(Vector3.right, -dirToStone),
            horizontalReflectCondition || verticalReflectCondition
            );
        Quaternion swingRotation = QuaternionUtility.Conditional(
            ReflectBy180 * RotBy90,
            RotBy90,
            horizontalReflectCondition || verticalReflectCondition
            );
        miningAnimation.rotations = new List<Quaternion> 
        { 
            swingRotation,
            readyRotation,
            swingRotation,
        };
        miningAnimation.interpolationTypes = new List<InterpolationType>
        {
            InterpolationType.EaseOutElastic,
            InterpolationType.EaseInOutElastic,
            InterpolationType.EaseInExp,
        };
        miningAnimation.loop = true;
        tool.objectAnimator.CreateAndPlayAnimation(miningAnimation);
    }
    public void ReserveCell(CellData cellData)
    {
        //Reserve next cell in path segments
    }
    void Update()
    {
        agentPositionBuffer.UpdateBuffer(Time.deltaTime, agentCellCenterPos);
    }
    void FixedUpdate()
    {
        if (order == null)
            return;
        order.Navigate(Time.fixedDeltaTime * movementSpeed, transform);
    }
}