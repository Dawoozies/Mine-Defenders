using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding 
{
    public static List<Vector3Int> aStar(Vector3Int start, Vector3Int end, Tilemap[] environments, Hashtable reservedTiles)
    {

        Dictionary<Vector3Int, PathNode> allNodes = new Dictionary<Vector3Int, PathNode>();
        PathNode startNode = new PathNode(start, null, end);
        startNode.g = 0;
        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode>(32 * 32);



        openList.Enqueue(startNode, startNode.f);
        allNodes.Add(startNode.position, startNode);
        while (openList.Count > 0)
        {
            
            PathNode current = openList.First;
            if (current.position == end)
            {
                return reconstructPath(current);
            }

            openList.Dequeue();
            List<PathNode> children = new List<PathNode>();
            foreach (Vector3Int direction in GameManager.ins.navDirections)
            {
                Vector3Int nodePosition = current.position + direction;
                if (!GameManager.ins.isInLevelBounds(nodePosition))
                {
                    continue;
                }
                bool isWalkable = true;
                foreach (Tilemap environment in environments)
                {
                    if (environment.GetTile(nodePosition) != null)
                    {
                        isWalkable = false;
                        break;
                    }
                }
                if (isWalkable)
                {
                    if (!allNodes.ContainsKey(nodePosition))
                    {
                        allNodes.Add(nodePosition, new PathNode(nodePosition, current, end));

                    }
                    children.Add(allNodes[nodePosition]);

                }

            }
            
            foreach (PathNode child in children)
            {
                float reservedCost = (reservedTiles != null && reservedTiles.ContainsKey(child.position)) ? 0.5f : 0f;
                float tentativeGScore = current.g + 1 + reservedCost;
                if (tentativeGScore < child.g)
                {
                    child.parent = current;
                    child.g = tentativeGScore;
                    child.f = tentativeGScore + child.h;
                    if (!openList.Contains(child))
                    {
                        openList.Enqueue(child, child.f);
                    }
                    else
                    {
                        openList.UpdatePriority(child, child.f);
                    }
                }
            }
        }
        return reconstructPath(null);
    }
    static List<Vector3Int> reconstructPath(PathNode current)
    {
        PathNode next = current;
        List<Vector3Int> path = new List<Vector3Int>();
        while(next != null)
        {
            path.Add((Vector3Int)next.position);
            next = next.parent;
        }
        return path;
    }
}

public class PathNode : FastPriorityQueueNode
{
    public float g;
    public float f;
    public float h;
    public Vector3Int position;
    public PathNode parent;

    public PathNode(Vector3Int position, PathNode parent, Vector3Int endPosition) {
        this.position = position;
        this.parent = parent;
        h = Mathf.Abs((endPosition - position).x) + Mathf.Abs((endPosition - position).y);
        g = f = float.MaxValue;
    }
}
