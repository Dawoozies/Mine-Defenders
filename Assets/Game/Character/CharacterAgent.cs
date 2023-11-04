using System.Collections.Generic;
using UnityEngine;

public class CharacterAgent : MonoBehaviour
{
    public LinkedList<MoveOrder> moveOrders;
    public float speed;

    public bool isEnemy;

    public delegate void NewMovementOrderHandler(string newOrderName);
    public event NewMovementOrderHandler NewMovementOrderEvent;
    public delegate void MovementCompleteHandler(string completedOrderName);
    public event MovementCompleteHandler MovementCompleteEvent;
    public delegate void MovementInterruptedHandler(string interruptedOrderName, string replacingOrderName);
    public event MovementInterruptedHandler MovementInterruptedEvent;
    void Start()
    {
        startPosition = transform.position;
        moveOrders = new LinkedList<MoveOrder>();
    }

    public void MovementOrder(string orderName, Vector3Int targetPosition) {

        if(moveOrders.First != null)
            MovementInterruptedEvent?.Invoke(moveOrders.First.Value.orderName, orderName);

        

        MoveOrder lastOrder = moveOrders.First != null ? moveOrders.First.Value : null;

        moveOrders.Clear();
        if (lastOrder != null) { 
            moveOrders.AddFirst(lastOrder);
        }

        
        List<Vector3Int> path = Pathfinding.aStar( 
            moveOrders.Count == 1 
            ? lastOrder.position 
            : GameManager.ins.WorldToCell(transform.position), 
            targetPosition,
            GameManager.ins.GetPlayerInaccessibleTilemaps(),
            null
            );
        
        for (int i = path.Count - 1; i >= 0; i--)
        {
            MoveOrder moveOrder = new MoveOrder("MoveToPosition", path[i]);
            if (orderName == "MoveToMiningTarget" && i == 0)
            {
                moveOrder.orderName = "MoveToMiningTarget";
            }
            moveOrders.AddLast(moveOrder);
        }
    }
    public void EnemyMovementOrder(string orderName, Vector3Int targetPosition)
    {
        if (moveOrders.First != null)
            MovementInterruptedEvent?.Invoke(moveOrders.First.Value.orderName, orderName);

        MoveOrder lastOrder = moveOrders.First != null ? moveOrders.First.Value : null;

        moveOrders.Clear();
        if (lastOrder != null)
        {
            moveOrders.AddFirst(lastOrder);
        }
        
        List<Vector3Int> path = Pathfinding.aStar(
            moveOrders.Count == 1
            ? lastOrder.position
            : GameManager.ins.WorldToCell(transform.position),
            targetPosition, 
            GameManager.ins.GetEnemyInaccessibleTilemaps(),
            GameManager.ins.reservedTiles
            );
        
        for (int i = path.Count - 1; i >= 0; i--)
        {
            MoveOrder moveOrder = new MoveOrder($"MovementOrder{i}", path[i]);
            moveOrders.AddLast(moveOrder);
        }
        if (orderName == "MoveToPlayer")
        {
            moveOrders.RemoveFirst();
        }
    }
    float interpolationProgress = 0;
    Vector3 startPosition;
    void FixedUpdate()
    {
        
        if (moveOrders.Count == 0)
            return;
        
       
        Vector3 nextPosition = GameManager.ins.CellToWorld(moveOrders.First.Value.position);
        // Debug.LogError(nextPosition);

        transform.position = Interpolation.Interpolate(startPosition, nextPosition, interpolationProgress, InterpolationType.Linear); //Fixed here to account for range of framerates
        interpolationProgress += Time.fixedDeltaTime * speed;
        //Debug.Log($"Distance to next point = {Vector3.Distance(transform.position, nextPosition)} Velocity = {velocity} Time.deltaTime = {Time.deltaTime}");
        if (interpolationProgress > 1f || (startPosition == nextPosition && !isEnemy))
        {
            startPosition = nextPosition;
            if(moveOrders.Count == 2 && isEnemy)
            {
                //This code block runs when enemies are within 1 tile of the player

            }
            if (moveOrders.Count == 1 && isEnemy)
            {
                //I dont think this code block runs hmm
                moveOrders.AddFirst(new MoveOrder(moveOrders.First.Value.orderName, GameManager.ins.WorldToCell(startPosition)));
                Debug.Log("Enemy adding move orders to become static");
            }

            if (!isEnemy)
            {
                MovementCompleteEvent?.Invoke(moveOrders.First.Value.orderName);
            }
            
            
            moveOrders.RemoveFirst();
            interpolationProgress = 0;
            
            if (!isEnemy)
            {
                return;
            }
            //Reserve
            if (GameManager.ins.WorldToCell(nextPosition) == moveOrders.First.Value.position)
            {
                return;
            }
            bool isPlayerReservingTarget = false;
            if (GameManager.ins.playerAgent.moveOrders.First == null) {
                isPlayerReservingTarget = GameManager.ins.playerLastCellPos == moveOrders.First.Value.position;
            }
            else
            {
                isPlayerReservingTarget = GameManager.ins.playerAgent.moveOrders.First.Value.position == moveOrders.First.Value.position;
            }

            if (isPlayerReservingTarget || !GameManager.ins.TryReserve(moveOrders.First.Value.position))
            {
                //insert move order which is where we are
                moveOrders.AddFirst(new MoveOrder(moveOrders.First.Value.orderName, GameManager.ins.WorldToCell(startPosition)));
            }
            else
            {
                //then reserve successful
                //release our last reservation
                GameManager.ins.ReleaseReservation(GameManager.ins.WorldToCell(startPosition));
            }
        }

    }
}
public class MoveOrder
{
    public string orderName;
    public Vector3Int position;
    public MoveOrder(string orderName, Vector3Int position)
    {
        this.orderName = orderName;
        this.position = position;
    }
}
