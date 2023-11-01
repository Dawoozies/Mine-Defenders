using System.Collections.Generic;
using UnityEngine;

public class CharacterAgent : MonoBehaviour
{

    
    public LinkedList<MoveOrder> currentMoveOrder;
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
        currentMoveOrder = new LinkedList<MoveOrder>();
    }

    public void MovementOrder(string orderName, Vector3Int targetPosition) {

        if(currentMoveOrder.First != null)
            MovementInterruptedEvent?.Invoke(currentMoveOrder.First.Value.orderName, orderName);

        

        MoveOrder lastOrder = currentMoveOrder.First != null ? currentMoveOrder.First.Value : null;

        currentMoveOrder.Clear();
        if (lastOrder != null) { 
            currentMoveOrder.AddFirst(lastOrder);
        }

        
        List<Vector3Int> path = Pathfinding.aStar( 
            currentMoveOrder.Count == 1 
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
            currentMoveOrder.AddLast(moveOrder);
        }
    }
    public void EnemyMovementOrder(string orderName, Vector3Int targetPosition)
    {
        if (currentMoveOrder.First != null)
            MovementInterruptedEvent?.Invoke(currentMoveOrder.First.Value.orderName, orderName);

        MoveOrder lastOrder = currentMoveOrder.First != null ? currentMoveOrder.First.Value : null;

        currentMoveOrder.Clear();
        if (lastOrder != null)
        {
            currentMoveOrder.AddFirst(lastOrder);
        }
        
        List<Vector3Int> path = Pathfinding.aStar(
            currentMoveOrder.Count == 1
            ? lastOrder.position
            : GameManager.ins.WorldToCell(transform.position),
            targetPosition, 
            GameManager.ins.GetEnemyInaccessibleTilemaps(),
            GameManager.ins.reservedTiles
            );
        
        for (int i = path.Count - 1; i >= 0; i--)
        {
            MoveOrder moveOrder = new MoveOrder(orderName, path[i]);
            currentMoveOrder.AddLast(moveOrder);
        }
        if (orderName == "MoveToPlayer")
        {
            currentMoveOrder.RemoveFirst();
        }
    }
    float interpolationProgress = 0;
    Vector3 startPosition;
    void FixedUpdate()
    {
        
        if (currentMoveOrder.Count == 0)
            return;
        
       
        Vector3 nextPosition = GameManager.ins.CellToWorld(currentMoveOrder.First.Value.position);
        // Debug.LogError(nextPosition);

        transform.position = Interpolation.Interpolate(startPosition, nextPosition, interpolationProgress, InterpolationType.Linear); //Fixed here to account for range of framerates
        interpolationProgress += Time.fixedDeltaTime * speed;
        //Debug.Log($"Distance to next point = {Vector3.Distance(transform.position, nextPosition)} Velocity = {velocity} Time.deltaTime = {Time.deltaTime}");
        if (interpolationProgress > 1f || (startPosition == nextPosition && !isEnemy))
        {
            startPosition = nextPosition;
            if (currentMoveOrder.Count == 1 && isEnemy)
            {
                currentMoveOrder.AddFirst(new MoveOrder(currentMoveOrder.First.Value.orderName, GameManager.ins.WorldToCell(startPosition)));
            }

            
            if (!isEnemy)
            {
                MovementCompleteEvent?.Invoke(currentMoveOrder.First.Value.orderName);
            }
            
            
            currentMoveOrder.RemoveFirst();
            interpolationProgress = 0;
            
            if (!isEnemy)
            {
                return;
            }
            
            //Reserve
            if (GameManager.ins.WorldToCell(nextPosition) == currentMoveOrder.First.Value.position)
            {
                return;
            }
            bool isPlayerReservingTarget = false;
            if (GameManager.ins.playerAgent.currentMoveOrder.First == null) {
                isPlayerReservingTarget = GameManager.ins.playerLastCellPos == currentMoveOrder.First.Value.position;
            }
            else
            {
                isPlayerReservingTarget = GameManager.ins.playerAgent.currentMoveOrder.First.Value.position == currentMoveOrder.First.Value.position;
            }

            if (isPlayerReservingTarget || !GameManager.ins.TryReserve(currentMoveOrder.First.Value.position))
            {
                //insert move order which is where we are
                currentMoveOrder.AddFirst(new MoveOrder(currentMoveOrder.First.Value.orderName, GameManager.ins.WorldToCell(startPosition)));
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
