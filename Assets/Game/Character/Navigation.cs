using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class NavigationOrder
{
    public string orderName;
    List<PathSegment> path;
    int segment;
    public bool onFirstSegment {
        get{
            if (path == null || path.Count == 0)
                return false;
            return segment == 0;
        }
    }
    public bool onFinalSegment
    {
        get{ 
            return segment == path.Count - 1;
        }
    }
    public bool complete
    {
        get{
            return onFinalSegment && path[segment].completed;
        }
    }
    public NavigationOrder(string orderName, Vector3Int x, Vector3Int y, Tilemap[] noMoveTilemaps, bool useReserved, int pathCutoff, InterpolationType interpolationType)
    {
        List<Vector3Int> pathPoints = 
            Pathfinding.aStar(x, y, noMoveTilemaps, useReserved ? GameManager.ins.reservedTiles : null);

        path = new List<PathSegment>();
        for (int i = pathPoints.Count - 1; i > pathCutoff ; i--)
        {
            //pathPoints[pathPoints.Count - 1] : Starting point
            //pathPoints[0] : End point
            var pathSegment = new PathSegment(pathPoints[i], pathPoints[i-1], interpolationType);
            path.Add(pathSegment);
        }
        segment = 0;
    }
    public void Navigate(float timeDelta, Transform transform)
    {
        transform.position = path[segment].TraverseSegment(timeDelta);
        if (path[segment].completed)
            segment++;
    }
}
public struct PathSegment
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