using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class NavigationOrder
{
    IAgent agent;
    public string orderName;
    List<PathSegment> segments;
    int segment;
    int pathCutoff;
    public bool allowDebug;

    public bool onFirstSegment {
        get{
            if (segments == null || segments.Count == 0)
                return false;
            return segment == 0;
        }
    }
    public bool onFinalSegment
    {
        get{ 
            return segment == segments.Count - 1;
        }
    }
    public bool complete
    {
        get{
            return onFinalSegment && segments[segment].completed;
        }
    }
    public delegate void OnNavigationComplete();
    public event OnNavigationComplete onNavigationComplete;
    public delegate void OnSegmentComplete(Vector3Int start, Vector3Int end); //return start and end points of segment
    public event OnSegmentComplete onSegmentComplete;
    public delegate void OnSegmentStart(Vector3Int start, Vector3Int end);
    public event OnSegmentStart onSegmentStart;
    public NavigationOrder(IAgent agent, string orderName, Vector3Int x, Vector3Int y, Tilemap[] notWalkable, bool useReserved, int pathCutoff, InterpolationType interpolationType)
    {
        onNavigationComplete = null;
        onSegmentComplete = null;
        onSegmentStart = null;
        List<Vector3Int> pathPoints = 
            Pathfinding.aStarNew(agent, x, y, notWalkable, useReserved ? GameManager.ins.reservedTiles : null);
        //Debug.Log(pathPoints.Count);
        segments = new List<PathSegment>();
        for (int i = pathPoints.Count - 1; i > 0 ; i--)
        {
            //pathPoints[pathPoints.Count - 1] : Starting point
            //pathPoints[0] : End point
            //Debug.Log($"i = {i} \n pathPoints[{i}] = {pathPoints[i]} \n pathPoints[{i-1}] = {pathPoints[i-1]}");
            var pathSegment = new PathSegment(pathPoints[i], pathPoints[i-1], interpolationType);
            segments.Add(pathSegment);
        }
        segment = 0;
        this.pathCutoff = pathCutoff;
    }
    public NavigationOrder(IAgent agent, string orderName, List<Vector3Int> calculatedPath, Tilemap[] notWalkable, bool useReserved, int pathCutoff, InterpolationType interpolationType)
    {
        if (calculatedPath == null)
            return;
        onNavigationComplete = null;
        onSegmentComplete = null;
        onSegmentStart = null;
        segments = new List<PathSegment>();
        for (int i = calculatedPath.Count - 1; i > 0; i--)
        {
            //Debug.Log($"i = {i} \n pathPoints[{i}] = {calculatedPath[i]} pathPoints[{i - 1}] = {calculatedPath[i - 1]}");
            var pathSegment = new PathSegment(calculatedPath[i], calculatedPath[i - 1], interpolationType);
            segments.Add(pathSegment);
        }
        //Debug.Log(segments.Count);
        segment = 0;
        this.pathCutoff = pathCutoff;
    }
    public NavigationOrder(IAgent agent)
    {
        this.agent = agent;
    }
    public void RefreshPath(Vector3Int target, bool moveNextTo)
    {
        List<Vector3Int> path = Pathfind(agent, target, moveNextTo);
        segments = new List<PathSegment>();
        for (int i = path.Count - 1; i > 0; i--)
        {
            var pathSegment = new PathSegment(path[i], path[i - 1], agent.args.moveInterpolationType);
            segments.Add(pathSegment);
        }
        segment = 0;
    }
    List<Vector3Int> Pathfind(IAgent agent, Vector3Int end, bool moveNextTo)
    {
        List<Vector3Int> pathPoints = new List<Vector3Int>();
        if(moveNextTo)
        {
            //then compute adjacent
            CellData cellDataAtEnd = GameManager.ins.GetCellDataAtPosition(end);
            pathPoints = cellDataAtEnd.GetPathToClosestCardinalNeighbour(agent);
        }
        else
        {
            pathPoints = Pathfinding.aStarNew(agent, agent.args.cellPos, end, agent.args.notWalkable, agent.args.reservedTiles);
        }
        return pathPoints;
    }
    //Input is timeDelta + transform you want to move
    public void Navigate(float timeDelta, Transform transform)
    {
        if (segments == null || segments.Count == 0)
            return;
        //Debug.Log($"segment = {segment} segments.Count = {segments.Count}");
        if(segment < segments.Count - pathCutoff)
        {
            if (segments[segment].completed)
            {
                Vector3Int start = segments[segment].x;
                Vector3Int end = segments[segment].y;
                onSegmentComplete?.Invoke(start, end);
                segment++;
            }
            else
            {
                if (allowDebug)
                {
                    Debug.Log($"Segment = {segment}");
                    segments[segment].allowDebug = true;
                }

                if (segments[segment].t == 0)
                {
                    Vector3Int start = segments[segment].x;
                    Vector3Int end = segments[segment].y;
                    onSegmentStart?.Invoke(start, end);
                }
                transform.position = segments[segment].TraverseSegment(timeDelta);
            }
        }
        else
        {
            //path should be complete
            if(onNavigationComplete != null)
            {
                onNavigationComplete?.Invoke();
                onNavigationComplete = null;
            }
            else
            {
                return;
            }
        }
    }
}
public class PathSegment
{
    public Vector3Int x { get; }
    public Vector3Int y { get; }
    InterpolationType interpolationType;
    public float t;
    public bool completed { get { return t >= 1; } }
    public bool allowDebug;
    public PathSegment(Vector3Int x, Vector3Int y, InterpolationType interpolationType)
    {
        this.x = x;
        this.y = y;
        this.interpolationType = interpolationType;
        t = 0;
    }
    public Vector3 TraverseSegment(float timeDelta)
    {
        var p = Interpolation.Interpolate(x, y, t, interpolationType);
        t += timeDelta;
        if(allowDebug)
            Debug.Log($"Time = {t}");
        return p;
    }
}