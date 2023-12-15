using ItemFactory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Material = ItemFactory.Material;

public class Player : MonoBehaviour, IAgent
{
    public int movementPerTurn;
    public float moveInterpolationSpeed;
    public InterpolationType moveInterpolationType;
    Vector3Int agentCellPos { get { return GameManager.ins.WorldToCell(transform.position); } }
    Vector3 agentCellCenterPos { get { return GameManager.ins.WorldToCellCenter(transform.position); } }

    AgentArgs IAgent.args { get { return agentData; } }
    AgentArgs agentData;

    Buffer<Vector3> agentPositionBuffer;
    #region Tool Definition
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
    #endregion

    public Vector3 worldPos => agentCellCenterPos;
    public Vector3Int cellPos => agentCellPos;
    private void Awake()
    {
        //Set up agentData
        agentData = new AgentArgs(transform, AgentType.Player, this);
        agentData.movementPerTurn = movementPerTurn;
        agentData.moveInterpolationSpeed = moveInterpolationSpeed;
        agentData.moveInterpolationType = moveInterpolationType;
        agentData.movesLeft = movementPerTurn;
        agentData.allowedToLoot = LootType.All;
        agentData.target = null;
        agentData.targetedBy = new List<IAgent>();

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

        //On tap we should interrupt mining animation if we are doing that
        GameManager.onTap += (CellData cellData) =>
        {
            if (tool.objectAnimator.GetCurrentAnimationName() == "Mining")
            {
                tool.objectAnimator.PlayAnimation("NotMining");
                tool.basePartRenderer.color = Color.clear;
                tool.materialPartRenderer.color = Color.clear;
            }
        };
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

    public Tilemap[] GetInaccessibleTilemaps()
    {
        return GameManager.ins.GetPlayerInaccessibleTilemaps();
    }
    //public void DamageAgent(Attack attack)
    //{

    //}
    public bool HasValidTarget()
    {
        if (agentData.target == null)
            return false;
        if (agentData.target.args.isDead)
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