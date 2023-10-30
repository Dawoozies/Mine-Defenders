using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAgent : MonoBehaviour
{
    [Serializable]
    public class MoveOrder
    {
        public string orderName;
        public Vector3 destination;
        public float distanceRequired;
        public MoveOrder(string orderName, Vector3 destination)
        {
            this.orderName = orderName;
            this.destination = destination;
        }
        public MoveOrder(string orderName, Vector3 destination, float distanceRequired)
        {
            this.orderName = orderName;
            this.destination = destination;
            this.distanceRequired = distanceRequired;
        }
        public float DistanceLeft(Vector3 currentPos)
        {
            currentPos.z = 0;
            return Vector3.Distance(currentPos, destination);
        }
        public bool WithinRequiredDistance(Vector3 currentPos)
        {
            currentPos.z = 0;
            return Vector3.Distance(currentPos, destination) <= distanceRequired;
        }
    }
    NavMeshAgent agent;
    MoveOrder currentMoveOrder;
    public delegate void NewMovementOrderHandler(string newOrderName);
    public event NewMovementOrderHandler NewMovementOrderEvent;
    public delegate void MovementCompleteHandler(string completedOrderName);
    public event MovementCompleteHandler MovementCompleteEvent;
    public delegate void MovementInterruptedHandler(string interruptedOrderName, string replacingOrderName);
    public event MovementInterruptedHandler MovementInterruptedEvent;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }
    public void MovementOrder(string orderName, Vector3 targetPosition)
    {
        targetPosition.z = 0;
        if(currentMoveOrder != null)
        {
            //If current move order is not null then we are interrupting an ongoing move order
            MovementInterruptedEvent?.Invoke(currentMoveOrder.orderName, orderName);
        }

        currentMoveOrder = new MoveOrder(orderName, targetPosition);
        agent.SetDestination(targetPosition);
        NewMovementOrderEvent?.Invoke(orderName);
    }
    public void MoveWithinDistanceOrder(string orderName, Vector3 targetPosition, float distanceRequired)
    {
        targetPosition.z = 0;
        if (currentMoveOrder != null)
        {
            //If current move order is not null then we are interrupting an ongoing move order
            MovementInterruptedEvent?.Invoke(currentMoveOrder.orderName, orderName);
        }

        currentMoveOrder = new MoveOrder(orderName, targetPosition, distanceRequired);
        agent.SetDestination(targetPosition);
        NewMovementOrderEvent?.Invoke(orderName);
    }
    public void StopMovement()
    {
        if (currentMoveOrder != null)
            MovementInterruptedEvent?.Invoke(currentMoveOrder.orderName, "StopMovementCall");
    }
    void Update()
    {
        if (currentMoveOrder == null)
            return;

        if (currentMoveOrder.distanceRequired > 0)
        {
            if (currentMoveOrder.WithinRequiredDistance(transform.position))
            {
                MovementCompleteEvent?.Invoke(currentMoveOrder.orderName);
                Debug.Log("Movement completed within required distance");
                //Then eject movement order
                currentMoveOrder = null;
                agent.ResetPath();
            }
        }
        else
        {
            if(currentMoveOrder.DistanceLeft(transform.position) <= 0.01f)
            {
                MovementCompleteEvent?.Invoke(currentMoveOrder.orderName);
                Debug.Log("Movement complete");
                //Then eject movement order
                currentMoveOrder = null;
                agent.ResetPath();
            }
        }
    }
}
