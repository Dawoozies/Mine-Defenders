using System.Collections.Generic;
using UnityEngine;

public class CharacterAgent : MonoBehaviour
{

    public class MoveOrder
    {
        public string orderName;
        public List<Vector3Int> path;
        public int pathCount => path.Count;
        public MoveOrder(string orderName, List<Vector3Int> path)
        {
            this.orderName = orderName;
            this.path = path;
        }
    }
    MoveOrder currentMoveOrder;
    public float speed;

    public delegate void NewMovementOrderHandler(string newOrderName);
    public event NewMovementOrderHandler NewMovementOrderEvent;
    public delegate void MovementCompleteHandler(string completedOrderName);
    public event MovementCompleteHandler MovementCompleteEvent;
    public delegate void MovementInterruptedHandler(string interruptedOrderName, string replacingOrderName);
    public event MovementInterruptedHandler MovementInterruptedEvent;


    public void MovementOrder(string orderName, Vector3Int targetPosition) {
        if(currentMoveOrder != null) 
        {
            MovementInterruptedEvent?.Invoke(currentMoveOrder.orderName, orderName);
        }
        List<Vector3Int> path = Pathfinding.aStar(GameManager.ins.WorldToCell(transform.position), targetPosition,GameManager.ins.GetNonWalkableTilemaps());
        currentMoveOrder = new MoveOrder(orderName, path);
    }
    void Update()
    {
        if (currentMoveOrder == null)
            return;

        if(currentMoveOrder.path != null && currentMoveOrder.pathCount > 0)
        {
            Vector3 nextPosition = GameManager.ins.CellToWorld(currentMoveOrder.path[currentMoveOrder.pathCount - 1]);
           // Debug.LogError(nextPosition);
            if (Vector3.Distance(transform.position, nextPosition) < 0.01f )
            {
                currentMoveOrder.path.RemoveAt(currentMoveOrder.pathCount - 1);
                if(currentMoveOrder.pathCount == 0 )
                {
                    MovementCompleteEvent?.Invoke(currentMoveOrder.orderName);
                }
            }

            Vector3 velocity = Vector3.Normalize(nextPosition - transform.position)*speed;
            transform.position += velocity * Time.deltaTime;
        }
    }
}
