using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : MonoBehaviour
{
    CharacterAgent agent;
    Buffer<Vector3> positionBuffer;
    SpriteRenderer spriteRenderer;
    SpriteRenderer pickaxeSpriteRenderer;
    public ObjectAnimator pickaxeAnimator;
    public Vector3 miningTargetPos;
    public int pickaxeDamage;
    void Start()
    {
        GridInteraction.GridInteractEvent += PlayerTap;
        agent = GetComponent<CharacterAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickaxeSpriteRenderer = pickaxeAnimator.GetComponent<SpriteRenderer>();
        pickaxeSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

        positionBuffer = new Buffer<Vector3>(transform.position, 0.125f);
        positionBuffer.onWriteToBuffer += () => { GameManager.OnPlayerPositionUpdated += positionBuffer.GetBuffer; };
        agent.MovementCompleteEvent += StartMiningOrder;
        pickaxeAnimator.TimeUpdateEvent += (float time, int currentIndex,string animName) => {
            if (animName != "SwingPickaxeAtStone")
                return;
            if(currentIndex == 1)
            {
                GridInteraction.OnDamageDurability += () => {
                    return (GameManager.ins.WorldToCell(miningTargetPos), pickaxeDamage);
                };
            }
        };
        GridInteraction.StoneDestroyedEvent += (StoneDestroyedArgs args) => {
            if (args.CellWorldPosition == miningTargetPos)
            {
                pickaxeAnimator.PlayAnimation("NotMining");
                pickaxeSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            }
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
        pickaxeSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        if (args.durability > 0)
        {
            //Then we have tapped on stone
            agent.MoveWithinDistanceOrder("MoveToMiningTarget", args.CellWorldPosition, 0.75f);
            miningTargetPos = args.CellWorldPosition;
        }
        else
        {
            agent.MovementOrder("MoveToPosition", args.CellWorldPosition);
            //We need to clear this

            miningTargetPos = Vector3.zero;
        }
    }
    void StartMiningOrder(string completedMovementOrderName)
    {
        if(completedMovementOrderName == "MoveToMiningTarget")
        {
            //Then we are within range of mining
            //Debug.Log("Start mining!");
            pickaxeSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 10;

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