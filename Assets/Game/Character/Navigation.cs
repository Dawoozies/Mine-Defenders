using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class NavigationOrder
{
    public string orderName;
    List<PathSegment> segments;
    int segment;
    int pathCutoff;
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
    public NavigationOrder(string orderName, Vector3Int x, Vector3Int y, Tilemap[] notWalkable, bool useReserved, int pathCutoff, InterpolationType interpolationType)
    {
        onNavigationComplete = null;
        List<Vector3Int> pathPoints = 
            Pathfinding.aStar(x, y, notWalkable, useReserved ? GameManager.ins.reservedTiles : null);
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
    public NavigationOrder(string orderName, List<Vector3Int> calculatedPath, Tilemap[] notWalkable, bool useReserved, int pathCutoff, InterpolationType interpolationType)
    {
        onNavigationComplete = null;
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
    //Input is timeDelta + transform you want to move
    public void Navigate(float timeDelta, Transform transform)
    {
        if (segments == null || segments.Count == 0)
            return;
        //Debug.Log($"segment = {segment} segments.Count = {segments.Count}");
        if(segment < segments.Count - pathCutoff)
        {
            if (segments[segment].completed)
                segment++;
            else
                transform.position = segments[segment].TraverseSegment(timeDelta);
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
    Vector3 x;
    Vector3 y;
    InterpolationType interpolationType;
    float t;
    public bool completed{ get{ return t >= 1;} }
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
        return p;
    }
}