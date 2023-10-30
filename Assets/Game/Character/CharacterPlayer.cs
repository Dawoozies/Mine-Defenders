using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : MonoBehaviour
{
    CharacterAgent agent;
    Buffer<Vector3> positionBuffer;
    public ObjectAnimator pickaxeAnimator;
    public Vector3 miningTargetPos;
    void Start()
    {
        GridInteraction.GridInteractEvent += PlayerTap;
        agent = GetComponent<CharacterAgent>();
        positionBuffer = new Buffer<Vector3>(transform.position, 1f);
        positionBuffer.onWriteToBuffer += () => { GameManager.OnPlayerPositionUpdated += positionBuffer.GetBuffer; };
        agent.MovementCompleteEvent += StartMiningOrder;
    }
    void Update()
    {
        positionBuffer.UpdateBuffer(Time.deltaTime, transform.position);
    }
    void PlayerTap(GridInteractArgs args)
    {
        //agent.MovementOrder("MoveToPosition", args.CellWorldPosition);
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
            Debug.Log("Start mining!");
            ObjectAnimation pickaxeAnimation = new ObjectAnimation();
            pickaxeAnimation.animName = "SwingPickaxeAtStone";
            pickaxeAnimation.frames = 2;
            pickaxeAnimation.positions = new List<Vector3> { 
                transform.position,
                miningTargetPos,
            };
            pickaxeAnimation.interpolationTypes = new List<InterpolationType>
            {
                InterpolationType.EaseOutExp,
                InterpolationType.EaseOutExp,
                InterpolationType.EaseInExp
            };
            pickaxeAnimation.loop = true;
            pickaxeAnimator.CreateAndPlayAnimation(pickaxeAnimation);
        }
    }
}
