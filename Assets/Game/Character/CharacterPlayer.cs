using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : MonoBehaviour
{
    CharacterAgent agent;
    Buffer<Vector3> positionBuffer;
    public ObjectAnimator pickaxeAnimator;
    public Vector3 miningTargetPos;
    public int pickaxeDamage;
    void Start()
    {
        GridInteraction.GridInteractEvent += PlayerTap;
        agent = GetComponent<CharacterAgent>();
        positionBuffer = new Buffer<Vector3>(transform.position, 1f);
        positionBuffer.onWriteToBuffer += () => { GameManager.OnPlayerPositionUpdated += positionBuffer.GetBuffer; };
        agent.MovementCompleteEvent += StartMiningOrder;
        pickaxeAnimator.LoopCompleteEvent += (ObjectAnimator animator, string completedAnimName) => {
            if (completedAnimName != "SwingPickaxeAtStone")
                return;
            GridInteraction.OnDamageDurability += () => {
                return (GameManager.ins.WorldToCell(miningTargetPos), pickaxeDamage);
            };
        };
    }
    void Update()
    {
        positionBuffer.UpdateBuffer(Time.deltaTime, transform.position);
    }
    void PlayerTap(GridInteractArgs args)
    {
        //agent.MovementOrder("MoveToPosition", args.CellWorldPosition);
        //we should interrupt pickaxe animator
        pickaxeAnimator.PlayAnimation("NotMining");
        if(args.durability > 0)
        {
            //Then we have tapped on stone
            agent.MoveWithinDistanceOrder("MoveToMiningTarget", args.CellWorldPosition, 0.75f);
            miningTargetPos = args.CellWorldPosition;
        }
        else
        {
            agent.MovementOrder("MoveToPosition", args.CellWorldPosition);
        }
    }
    void StartMiningOrder(string completedMovementOrderName)
    {
        if(completedMovementOrderName == "MoveToMiningTarget")
        {
            //Then we are within range of mining
            //Debug.Log("Start mining!");

            ObjectAnimation pickaxeAnimation = new ObjectAnimation();
            pickaxeAnimation.animName = "SwingPickaxeAtStone";
            pickaxeAnimation.frames = 3;
            Vector3 directionToStone = Vector3.Normalize(miningTargetPos - GameManager.ins.WorldToCellCenter(transform.position));
            pickaxeAnimation.positions = new List<Vector3> { 
                Vector3.zero,
                -directionToStone*0.5f,
                directionToStone*0.65f,
            };
            
            Quaternion RotationBy90 = Quaternion.AngleAxis(90, Vector3.forward);
            Quaternion ReflectionBy180 = Quaternion.AngleAxis(180, Vector3.up);
            bool horizontalReflectCondition = Mathf.Min(0, Mathf.Sign(Vector3.Dot(-directionToStone, Vector3.right))) < 0;
            bool verticalReflectCondition = Mathf.Min(0, Mathf.Sign(Vector3.Dot(directionToStone, Vector3.up))) < 0;
            Quaternion readyRotation = QuaternionUtility.Conditional(
                ReflectionBy180,
                Quaternion.FromToRotation(Vector3.right, -directionToStone),
                horizontalReflectCondition || verticalReflectCondition
                );
            Quaternion swingRotation = QuaternionUtility.Conditional(
                ReflectionBy180*RotationBy90,
                RotationBy90,
                horizontalReflectCondition || verticalReflectCondition
                );
            pickaxeAnimation.rotations = new List<Quaternion>
            {
                swingRotation,
                readyRotation,
                swingRotation,
            };
            pickaxeAnimation.interpolationTypes = new List<InterpolationType>
            {
                InterpolationType.EaseOutElastic,
                InterpolationType.EaseInOutElastic,
                InterpolationType.EaseInExp
            };
            pickaxeAnimation.loop = true;
            pickaxeAnimator.CreateAndPlayAnimation(pickaxeAnimation);
        }
    }
}