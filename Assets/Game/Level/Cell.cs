using System;
using System.Collections.Generic;
using UnityEngine;
public class CellData
{
    public Vector3Int cellPosition;
    public Vector3 cellCenterWorldPosition;
    //public bool isPlayerSpawnArea;
    //public bool isLevelBoundary;
    public bool isHidden;
    public CellAreaFlags cellAreaFlags;
    public Ore ore;
    public int durability;
    public bool isPit;
    public bool isUncoveredPit;
    public bool isPitCenter;
    public CellLoot loot;
    public List<CellData> neighbours;
    public bool isTraversable()
    {
        if (durability > 0)
            return false;
        if (isPit || isUncoveredPit || isPitCenter)
            return false;
        if (cellAreaFlags.HasFlag(CellAreaFlags.LevelBoundary))
            return false;
        return true;
    }
    public int traversableNeighbours()
    {
        if (neighbours == null)
            neighbours = GetCardinalNeighbours(false);
        if (neighbours == null)
            return 0;
        int traversableNeighbours = 0;
        foreach (CellData item in neighbours)
        {
            if (item == null)
                continue;
            if (item.isTraversable())
                traversableNeighbours++;
        }
        return traversableNeighbours;
    }
    public List<CellData> GetAllNeighbours(bool includeSelf)
    {
        return GameManager.ins.GetAllNeighboursAroundCell(cellPosition, includeSelf);
    }
    public List<CellData> GetCardinalNeighbours(bool includeSelf)
    {
        if (neighbours != null)
            return neighbours;
        neighbours = GameManager.ins.GetCardinalNeighboursAroundCell(cellPosition, false);
        return neighbours;
    }
    public List<Vector3Int> GetPathToClosestCardinalNeighbour(IAgent agent)
    {
        List<CellData> neighbours = GetCardinalNeighbours(true);
        List<Vector3Int> shortestPath = null;
        foreach (CellData neighbour in neighbours)
        {
            List<Vector3Int> path;
            path = Pathfinding.aStarWithIgnore(agent.args.cellPos, neighbour.cellPosition, agent.GetInaccessibleTilemaps(), null);
            if (path == null || path.Count == 0)
                continue;
            if (shortestPath == null)
                shortestPath = path;
            else
                shortestPath = path.Count < shortestPath.Count ? path : shortestPath;
        }
        return shortestPath;
    }
    public List<Vector3Int> GetPathToCell(IAgent agent)
    {
        if (durability > 0)
        {
            return GetPathToClosestCardinalNeighbour(agent);
        }
        List<Vector3Int> path = Pathfinding.aStarWithIgnore(agent.args.cellPos, cellPosition, agent.GetInaccessibleTilemaps(), null);
        return path;
    }
}
[Flags]
public enum CellAreaFlags
{
    None = 0,
    PlayerSpawnArea = 1,
    LevelBoundary = 1 << 1,
    IsEnemySpawnArea = 1 << 2,
    IsEnemySpawnCenter = 1 << 3,
}
[Flags]
public enum CellStructureFlags
{
    None = 0,
    Stone = 1,
    Wall = 1 << 1,
    Portal = 1 << 2,
}
[Flags]
public enum CellFloorFlags
{
    None = 0,
    Stone = 1,
    Grass = 1 << 1,
}