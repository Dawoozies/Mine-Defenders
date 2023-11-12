using ItemFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Material = ItemFactory.Material;

public class Player : MonoBehaviour, IAgent
{
    NavigationOrder order;
    public int moveSpeed;
    public InterpolationType moveInterpolationType;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }

    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;

    Buffer<Vector3> agentPositionBuffer;
    [Serializable]
    public class Tool
    {
        [HideInInspector]
        public Vector3Int toolTargetCellPos;
        [HideInInspector]
        public Vector3 toolTargetCellCenterWorldPos;
        public int baseDamage;
        public Equipment equipmentBase;
        public Material materialBase;
        public ObjectAnimator objectAnimator;
        public SpriteRenderer materialPartRenderer;
        public SpriteRenderer basePartRenderer;
    }
    public Tool tool;
    private void Awake()
    {
        //Set up agentData
        agentData = new AgentArgs(transform, AgentType.Player);
        agentData.moveSpeed = moveSpeed;
        agentData.notWalkable = GameManager.ins.GetPlayerInaccessibleTilemaps();
        agentData.reservedTiles = null;
        agentData.moveInterpolationType = moveInterpolationType;
        agentData.movesLeft = moveSpeed;
        agentPositionBuffer = new Buffer<Vector3>(agentCellCenterPos, 0.125f);
        agentPositionBuffer.onWriteToBuffer += () => {
            GameManager.OnPlayerPositionBufferUpdated += () => { return agentCellPos; };
        };

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
                    //tool.spriteAnimator.PlayAnimation("PutAway");
                    tool.basePartRenderer.color = Color.clear;
                    tool.materialPartRenderer.color = Color.clear;
                }
            }
        };
    }
    public void SetMiningOrder(CellData cellData)
    {
        tool.toolTargetCellPos = cellData.cellPosition;
        tool.toolTargetCellCenterWorldPos = cellData.cellCenterWorldPosition;
        List<Vector3Int> shortestPathToNeighbour
            = cellData.GetPathToClosestCardinalNeighbour(this);
        //If we are right next to what we want to mine then just start mining animation
        if(Vector3Int.Distance(agentCellPos, cellData.cellPosition) == 1)
        {
            StartMiningAnimation();
        }
        //Else we must use the code below
        if (shortestPathToNeighbour == null)
            return;
        order = new NavigationOrder(
            this,
            "MoveToMiningTarget",
            shortestPathToNeighbour,
            GameManager.ins.GetPlayerInaccessibleTilemaps(),
            false,
            0, //No cutoff since we are already pathing to an adjacent point
            moveInterpolationType
            );
        order.onNavigationComplete += StartMiningAnimation;
    }
    public void StartMiningAnimation()
    {
        #region Construct Pickaxe Sprite
        tool.basePartRenderer.sprite = tool.equipmentBase.basePart;
        tool.materialPartRenderer.sprite = tool.equipmentBase.materialPart;
        tool.basePartRenderer.color = Color.white;
        tool.materialPartRenderer.color = tool.materialBase.color;
        #endregion
        #region Create And Play Object Animation
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
        #endregion
    }
    void Update()
    {
        agentPositionBuffer.UpdateBuffer(Time.deltaTime, agentCellCenterPos);
    }
}