using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public interface IAgent
{
    public AgentArgs args { get; }
    public AgentNavigator navigator { get; }
    public AgentType AgentType { get; }
}
public enum AgentType
{
    Player = 0,
    Enemy = 1,
    Defender = 2,
}
public class AgentArgs
{
    public Transform transform { get; set; }
    public float moveSpeed;
    public Vector3Int cellPos { get => GameManager.ins.WorldToCell(transform.position); }
    public Vector3 worldPos { get => GameManager.ins.WorldToCellCenter(transform.position); }
    public Tilemap[] notWalkable;
    public Hashtable reservedTiles;
    public InterpolationType moveInterpolationType;
    public AgentPath path;
    public Vector3Int previousPoint;
    public bool hasInstruction;
    public AgentArgs(Transform transform)
    {
        this.transform = transform;
    }
    public void MoveAlongPath(float timeDelta)
    {
        if (path == null)
            return;
        if (path.completed)
        {
            previousPoint = path.start;
            if (hasInstruction)
                hasInstruction = false;
            return;
        }
        transform.position = path.Traverse(timeDelta * moveSpeed);
    }
}
//Dont need order name lmao
//just need event
public class AgentNavigator
{
    public IAgent agent;
    public CellData occupiedCell;
    public CellData reservedCell;
    public CellData targetCell;
    public List<Segment> segments;
    public int activeSegment;
    public AgentNavigator(IAgent agent)
    {
        this.agent = agent;
        occupiedCell = GameManager.ins.GetCellDataAtPosition(agent.args.cellPos);
        occupiedCell.TryOccupation(agent);
        //PLEASE DEAR GOD DON'T LET THINGS SPAWN ON EACH OTHER
    }
    public void SetNewTarget(CellData targetCell)
    {
        this.targetCell = targetCell;
        ComputePath();
    }
    public void ComputePath()
    {
        if(segments != null && segments.Count > 0)
        {
            //segments[activeSegment].start.ReleaseOccupation(agent);
            segments[activeSegment].end.ReleaseReservation(agent);
        }
        List<Vector3Int> path = targetCell.GetPathToCell(agent);
        segments = new List<Segment>();
        for (int i = path.Count - 1; i > 0; i--)
        {
            var pathSegment = new Segment(path[i], path[i - 1], agent.args.moveInterpolationType);
            segments.Add(pathSegment);
        }
        activeSegment = 0;
        if(segments != null && segments.Count > 0)
        {
            segments[activeSegment].start.TryOccupation(agent);
            segments[activeSegment].end.TryReservation(agent);
        }
        else
        {
            //compute path has resulted in null path
            //we still have to occupy where we are
            //GameManager.ins.GetCellDataAtPosition(agent.args.cellPos).TryOccupation(agent);
        }
    }
    public void Navigate(float timeDelta)
    {
        if (targetCell == null)
            return;
        if (segments == null || segments.Count == 0)
            return;
        //Do all the event triggering stuff later
        if(activeSegment >= segments.Count)
        {
            return;
        }
        bool occupationSucceeded = segments[activeSegment].start.TryOccupation(agent);
        bool reservationSucceeded = segments[activeSegment].end.TryReservation(agent);
        if(reservationSucceeded)
        {
            agent.args.transform.position = segments[activeSegment].TraverseSegment(timeDelta * agent.args.moveSpeed);
        }
        if (segments[activeSegment].completed)
        {
            segments[activeSegment].start.ReleaseOccupation(agent);
            if (agent.AgentType == AgentType.Player)
                GameManager.ins.Update_EnemyAgentsOnPlayerNewCell(segments[activeSegment].end);
            activeSegment++;
        }
    }
}
public class Segment
{
    public CellData start;
    public CellData end;
    public InterpolationType interpolationType;
    public float t;
    public bool completed { get { return t >= 1; } }
    public Segment(Vector3Int start, Vector3Int end, InterpolationType interpolationType)
    {
        this.start = GameManager.ins.GetCellDataAtPosition(start);
        this.end = GameManager.ins.GetCellDataAtPosition(end);
        this.interpolationType = interpolationType;
    }
    public Vector3 TraverseSegment(float timeDelta)
    {
        var p = Interpolation.Interpolate(start.cellPosition, end.cellPosition, t, interpolationType);
        t += timeDelta;
        return p;
    }
}
public class AgentPath
{
    public Vector3Int start;
    public Vector3Int end;
    public InterpolationType interpolationType;
    public float t;
    public bool completed { get { return t >= 1; } }
    public AgentPath(Vector3Int start, Vector3Int end, InterpolationType interpolationType)
    {
        this.start = start;
        this.end = end;
        this.interpolationType = interpolationType;
        t = 0;
    }
    public Vector3 Traverse(float timeDelta)
    {
        var p = Interpolation.Interpolate(start, end, t, interpolationType);
        t += timeDelta;
        return p;
    }
}