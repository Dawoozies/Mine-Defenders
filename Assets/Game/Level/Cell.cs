using System;
using System.Collections.Generic;
using UnityEngine;
public class Cell
{
    public Vector3Int position;
    public Vector3 centerWorldPosition;
    public CellAreaFlags areaFlags;
    public CellStructureFlags structureFlags;
    public CellFloorFlags floorFlags;
    public int durability;
    public List<CellContent> contents;
}
[Flags]
public enum CellAreaFlags
{
    None = 0,
    PlayerSpawnArea = 1,
    LevelBoundary = 2,
}
[Flags]
public enum CellStructureFlags
{
    None = 0,
    Stone = 1,
    Wall = 2,
}
[Flags]
public enum CellFloorFlags
{
    None = 0,
    Stone = 1,
    Grass = 2,
}