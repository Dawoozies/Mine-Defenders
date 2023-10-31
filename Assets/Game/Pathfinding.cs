using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding 
{
    public static List<Vector3Int> aStar(Vector3Int start, Vector3Int end, Tilemap[] environments) {

        PathNode startNode = new PathNode(start, null);
        PathNode endNode = new PathNode(end, null);

        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();
        List<Vector3Int> path = new List<Vector3Int>();
        

        openList.Add(startNode);

        while(openList.Count > 0){
            PathNode currentNode = openList[0];
            int currentIndex = 0;
            for (int i = 0; i < openList.Count; i++) {
                if (openList[i].f < currentNode.f) { 
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }
            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            if (currentNode.equals(endNode)) {
                path = new List<Vector3Int>();
                PathNode current = currentNode;
                while(current != null)
                {
                    path.Add(current.position);
                    current = current.parent;
                }
                return path;
            }

            List<PathNode> children = new List<PathNode>();
            foreach (Vector3Int direction in GameManager.ins.navDirections) {
                Vector3Int nodePosition = currentNode.position + direction;
                if(!GameManager.ins.isInLevelBounds(nodePosition))
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
                    children.Add(new PathNode(nodePosition, currentNode));
                }
                
            }

            foreach(PathNode child in children)
            {
                bool alreadyClosed = false;
                foreach(PathNode closed in closedList)
                {
                    if(closed.equals(child))
                    {
                        alreadyClosed = true;
                    }
                }
                if (alreadyClosed)
                {
                    continue;
                }
                child.g = currentNode.g + 1;
                child.h = (endNode.position - child.position).magnitude;
                child.f = child.g + child.h;
                bool alreadyOpen = false;
                foreach (PathNode open in openList)
                {
                    if (open.equals(child) && child.g > open.g)
                    {
                        alreadyOpen = true;
                    }
                }

                if (!alreadyOpen) {
                    openList.Add(child);
                }
            }
            
        }
        return path;
    }
}

public class PathNode
{
    public float g;
    public float f;
    public float h;
    public Vector3Int position;
    public PathNode parent;

    public PathNode(Vector3Int position, PathNode parent) {
        this.position = position;
        this.parent = parent;
        g = f = h = 0;
    }

    public bool equals(PathNode other)
    {
        return this.position.Equals(other.position);
    }
}
