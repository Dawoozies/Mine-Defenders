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
        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode>(64 * 64);



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
                foreach (Tilemap environment in environments) //Could merge tilemaps at the beginning to save time
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
    public static List<Vector3Int> aStarNew(IAgent agent, Vector3Int x, Vector3Int y, Tilemap[] notWalkable, Hashtable reservedTiles)
    {
        Dictionary<Vector3Int, PathNode> allNodes = new Dictionary<Vector3Int, PathNode>();
        PathNode startNode = new PathNode(x, null, y);
        startNode.g = 0;
        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode>(64*64);

        openList.Enqueue(startNode, startNode.f);
        allNodes.Add(startNode.position, startNode);
        while(openList.Count > 0)
        {
            PathNode current = openList.First;
            if (current.position == y)
            {
                return reconstructPath(current);
            }
            openList.Dequeue();
            List<PathNode> neighbourNodes = new List<PathNode>();
            List<CellData> neighbours = GameManager.ins.GetCardinalNeighboursAroundCell(current.position, false);
            foreach(CellData neighbour in neighbours)
            {
                if (!GameManager.ins.isInLevelBounds(neighbour.cellPosition))
                    continue;
                bool isWalkable = true;
                foreach (Tilemap item in notWalkable)
                {
                    if(item.GetTile(neighbour.cellPosition) != null)
                    {
                        isWalkable = false;
                        break;
                    }
                }
                if (isWalkable)
                {
                    if (!allNodes.ContainsKey(neighbour.cellPosition))
                        allNodes.Add(neighbour.cellPosition, new PathNode(neighbour.cellPosition, current, y));
                    neighbourNodes.Add(allNodes[neighbour.cellPosition]);
                }
            }
            foreach (PathNode neighbourNode in neighbourNodes)
            {
                //Make sure to include reserved costs here
                bool reservedTilesContainsKey = reservedTiles != null && reservedTiles.ContainsKey(neighbourNode.position);
                bool tileReservedByOther = reservedTilesContainsKey && reservedTiles[neighbourNode.position] != agent;
                float reservedCost = tileReservedByOther ? 0.5f : 0f;

                float tentativeGScore = current.g + 1 + reservedCost;
                if(tentativeGScore < neighbourNode.g)
                {
                    neighbourNode.parent = current;
                    neighbourNode.g = tentativeGScore;
                    neighbourNode.f = tentativeGScore + neighbourNode.h;
                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Enqueue(neighbourNode, neighbourNode.f);
                    }
                    else
                    {
                        openList.UpdatePriority(neighbourNode, neighbourNode.f);
                    }
                }
            }
        }
        return reconstructPath(null);
    }
    public static List<Vector3Int> aStarWithIgnore(Vector3Int start, Vector3Int end, Tilemap[] notWalkable, List<Vector3Int> cellsToIgnore)
    {
        Dictionary<Vector3Int, PathNode> allNodes = new Dictionary<Vector3Int, PathNode>();
        PathNode startNode = new PathNode(start, null, end);
        startNode.g = 0;
        FastPriorityQueue<PathNode> openList = new FastPriorityQueue<PathNode>(64 * 64);

        openList.Enqueue(startNode, startNode.f);
        allNodes.Add(startNode.position, startNode);
        while (openList.Count > 0)
        {
            PathNode current = openList.First;
            if (current.position == end)
                return reconstructPath(current);

            openList.Dequeue();
            List<PathNode> neighbourNodes = new List<PathNode>();
            List<CellData> neighbours = GameManager.ins.GetCardinalNeighboursAroundCell(current.position, false);
            foreach (CellData neighbour in neighbours)
            {
                if (!GameManager.ins.isInLevelBounds(neighbour.cellPosition))
                    continue;
                bool isWalkable = true;
                foreach (Tilemap item in notWalkable)
                {
                    if (item.GetTile(neighbour.cellPosition) != null)
                    {
                        isWalkable = false;
                        break;
                    }
                }
                if(cellsToIgnore != null && cellsToIgnore.Count > 0)
                {
                    if(cellsToIgnore.Contains(neighbour.cellPosition))
                    {
                        isWalkable = false;
                    }
                }
                if (isWalkable)
                {
                    if (!allNodes.ContainsKey(neighbour.cellPosition))
                        allNodes.Add(neighbour.cellPosition, new PathNode(neighbour.cellPosition, current, end));
                    neighbourNodes.Add(allNodes[neighbour.cellPosition]);
                }
            }
            foreach (PathNode neighbourNode in neighbourNodes)
            {
                //Make sure to include reserved costs here
                //bool reservedTilesContainsKey = reservedTiles != null && reservedTiles.ContainsKey(neighbourNode.position);
                //bool tileReservedByOther = reservedTilesContainsKey && reservedTiles[neighbourNode.position] != agent;
                //float reservedCost = tileReservedByOther ? 0.5f : 0f;

                float tentativeGScore = current.g + 1;
                if (tentativeGScore < neighbourNode.g)
                {
                    neighbourNode.parent = current;
                    neighbourNode.g = tentativeGScore;
                    neighbourNode.f = tentativeGScore + neighbourNode.h;
                    if (!openList.Contains(neighbourNode))
                        openList.Enqueue(neighbourNode, neighbourNode.f);
                    else
                        openList.UpdatePriority(neighbourNode, neighbourNode.f);
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