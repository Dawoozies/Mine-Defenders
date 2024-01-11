using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public Dictionary<Vector3Int, CellData> level;
    public Tilemap OreTilemap;
    public Tilemap StoneTilemap;
    public Tilemap StoneColorTilemap;
    public Tilemap HiddenTilemap;
    public Tilemap PitTilemap;
    public Tilemap FloorTilemap;
    List<Vector3Int> ignoreCells;

    public Vector2Int bottomLeftCorner;
    public Vector2Int topRightCorner;
    public Vector2Int spawnAreaBottomLeftCorner;
    public Vector2Int spawnAreaTopRightCorner;
    public int minPitDistanceFromSpawn;
    public int minPitDistanceFromEachOther;

    public RuleTile hiddenTile;
    public RuleTile floorTile;
    public RuleTile pitTile;
    public RuleTile ore;
    public RuleTile stone;
    public RuleTile stoneColor;
    public UnityEngine.Color defaultStoneColor;

    public Ore emptyOre;
    public List<Ore> ores;

    GridInformation GridInformation;
    public Vector2Int enemyPortalLocation;

    public int mapSize;
    public void GenerateLevel()
    {
        //Add all empty cellData
        //Hashtable cellTable = new Hashtable();
        level = new Dictionary<Vector3Int, CellData>();

        //all the way from outer boundary
        for (int x = bottomLeftCorner.x-1; x <= topRightCorner.x+1; x++)
        {
            for (int y = bottomLeftCorner.y-1; y <= topRightCorner.y+1; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                Vector3 cellCenterWorldPosition = GameManager.ins.CellToWorld(cellPosition);
                CellData cellData = new CellData();
                cellData.cellPosition = cellPosition;
                cellData.cellCenterWorldPosition = cellCenterWorldPosition;
                cellTable.Add(cellPosition, cellData);
            }
        }
        //Compute ignore list
        ignoreCells = new List<Vector3Int>();
        List<Vector3Int> levelBoundary = Boundaries(bottomLeftCorner - Vector2Int.one, topRightCorner + Vector2Int.one);
        List<Vector3Int> spawnBoundary = Boundaries(spawnAreaBottomLeftCorner + Vector2Int.one, spawnAreaTopRightCorner - Vector2Int.one);
        ignoreCells.AddRange(levelBoundary);
        ignoreCells.AddRange(spawnBoundary);
        //Setting level boundary
        foreach (Vector3Int cell in levelBoundary)
        {
            CellData cellData = (CellData)cellTable[cell];
            cellData.cellAreaFlags |= CellAreaFlags.LevelBoundary;
            cellData.ore = null;
            cellData.durability = 0;
        }
        //Setting spawn area
        List<Vector3Int> spawnArea = new List<Vector3Int>();
        for (int x = spawnAreaBottomLeftCorner.x + 1; x <= spawnAreaTopRightCorner.x - 1; x++)
        {
            for (int y = spawnAreaBottomLeftCorner.y + 1; y <= spawnAreaTopRightCorner.y - 1; y++)
            {
                Vector3Int cell = new Vector3Int(x,y,0);
                CellData cellData = (CellData)cellTable[cell];
                cellData.cellAreaFlags |= CellAreaFlags.PlayerSpawnArea;
                cellData.ore = null;
                cellData.durability = 0;
            }
        }
        //Generate Stone and do Ore walking
        for (int x = bottomLeftCorner.x; x <= topRightCorner.x ; x++)
        {
            for (int y = bottomLeftCorner.y; y <= topRightCorner.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                bool inSpawnArea =
                    (spawnAreaBottomLeftCorner.x < cell.x && cell.x < spawnAreaTopRightCorner.x &&
                    spawnAreaBottomLeftCorner.y < cell.y && cell.y < spawnAreaTopRightCorner.y);
                if (!inSpawnArea && x == 0 && y > 0 && y < enemyPortalLocation.y)
                {
                    GenerateFloor(cell);
                    ignoreCells.Add(cell);
                    continue;
                }
                bool ignoreCell = inSpawnArea || ignoreCells.Contains(cell);
                if(!ignoreCell)
                {
                    Ore highestRarityOreRolled = null;
                    foreach (Ore ore in ores)
                    {
                        if(ore.BaseRoll())
                        {
                            if(highestRarityOreRolled == null || ore.rarity > highestRarityOreRolled.rarity)
                            {
                                highestRarityOreRolled = ore;
                                continue;
                            }
                        }
                    }
                    if(highestRarityOreRolled != null)
                    {
                        int stepsRoll = highestRarityOreRolled.stepMinMax.x + Random.Range(0, highestRarityOreRolled.stepMinMax.y + 1);
                        CellWalker oreWalker = new CellWalker(cell, stepsRoll);
                        List<Vector3Int> walkedTiles = oreWalker.CalculateWalk(ignoreCells);
                        ignoreCells.AddRange(walkedTiles);
                        foreach (Vector3Int walkedTile in walkedTiles)
                        {
                            CellData cellData = (CellData)cellTable[walkedTile];
                            cellData.isPlayerSpawnArea = false;
                            cellData.isLevelBoundary = false;
                            cellData.ore = highestRarityOreRolled;
                            cellData.durability = highestRarityOreRolled.durability;

                            OreTilemap.SetTile(walkedTile, ore);
                            OreTilemap.SetColor(walkedTile, highestRarityOreRolled.color);
                        }
                    }
                    else
                    {
                        OreTilemap.SetTile(cell, ore);
                        OreTilemap.SetColor(cell, emptyOre.color);
                    }
                }
                if (!inSpawnArea)
                {
                    StoneTilemap.SetTile(cell, stone);
                    StoneColorTilemap.SetTile(cell, stoneColor);
                    StoneColorTilemap.SetColor(cell, defaultStoneColor);
                    HiddenTilemap.SetTile(cell, hiddenTile);

                    CellData cellData = (CellData)cellTable[cell];
                    cellData.isPlayerSpawnArea = false;
                    cellData.isLevelBoundary = false;
                    cellData.durability += 2;

                }
                if (inSpawnArea)
                {
                    GenerateFloor(cell);
                }
            }
        }
        //Generate Pits
        List<Vector2> sampleResults = 
            FastPoissonDiskSampling.Sampling(bottomLeftCorner, topRightCorner, minPitDistanceFromEachOther);
        if (sampleResults == null || sampleResults.Count == 0)
            Debug.LogError("Fast poisson disk sampling failed!");
        foreach (Vector2 sampledPoint in sampleResults)
        {
            Vector3Int cell = new Vector3Int(Mathf.RoundToInt(sampledPoint.x), Mathf.RoundToInt(sampledPoint.y), 0);
            bool inSpawnArea = 
                (spawnAreaBottomLeftCorner.x - minPitDistanceFromSpawn < cell.x && cell.x < spawnAreaTopRightCorner.x + minPitDistanceFromSpawn &&
                spawnAreaBottomLeftCorner.y - minPitDistanceFromSpawn < cell.y && cell.y < spawnAreaTopRightCorner.y + minPitDistanceFromSpawn);
            if (inSpawnArea || ignoreCells.Contains(cell))
                continue;
            List<CellData> neighbours = GameManager.ins.GetAllNeighboursAroundCell_InGivenHashtable(cellTable, cell, true);
            foreach (CellData cellData in neighbours)
            {
                cellData.isPit = true;
                cellData.isUncoveredPit = false;
                if (cellData.cellPosition == cell)
                    cellData.isPitCenter = true;
            }
            PitTilemap.SetTile(cell, pitTile);
        }
        return cellTable;
    }
    public void RemoveStoneTileAtCell(Vector3Int cellPos)
    {
        OreTilemap.SetTile(cellPos, null);
        StoneTilemap.SetTile(cellPos, null);
        StoneColorTilemap.SetTile(cellPos, null);
        HiddenTilemap.SetTile(cellPos, null);
    }
    List<Vector3Int> Boundaries(Vector2Int bottomLeft, Vector2Int topRight)
    {
        List<Vector3Int> boundaries = new List<Vector3Int>();
        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            //y fixed
            var bottomCell = new Vector3Int(x, bottomLeft.y, 0);
            var topCell = new Vector3Int(x, topRight.y, 0);
            if(!boundaries.Contains(bottomCell))
                boundaries.Add(bottomCell);
            if(!boundaries.Contains(topCell))
                boundaries.Add(topCell);
        }
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            //x fixed
            var bottomCell = new Vector3Int(bottomLeft.x, y, 0);
            var topCell = new Vector3Int(topRight.x, y, 0);
            if (!boundaries.Contains(bottomCell))
                boundaries.Add(bottomCell);
            if (!boundaries.Contains(topCell))
                boundaries.Add(topCell);
        }
        return boundaries;
    }
    public void GenerateFloor(Vector3Int cellPos)
    {
        FloorTilemap.SetTile(cellPos, floorTile);
    }
    public CellData UncoverFullPit(CellData uncoveredCell)
    {
        //Find the center of the pit first
        List<CellData> neighboursAroundUncover = GameManager.ins.GetAllNeighboursAroundCell(uncoveredCell.cellPosition, false);
        CellData pitCenter = neighboursAroundUncover[0];
        foreach (CellData neighbour in neighboursAroundUncover)
        {
            if(neighbour.isPitCenter)
            {
                pitCenter = neighbour;
                //Debug.Log($"Pit center at {neighbour.cellPosition}");
                break;
            }
        }
        //Ensure we get all tiles in the pit
        List<CellData> fullPit = GameManager.ins.GetAllNeighboursAroundCell(pitCenter.cellPosition, true);
        foreach (CellData pitCell in fullPit)
        {
            pitCell.isUncoveredPit = true;
            PitTilemap.SetTile(pitCell.cellPosition, pitTile);
            StoneTilemap.SetTile(pitCell.cellPosition, null);
            StoneColorTilemap.SetTile(pitCell.cellPosition, null);
            OreTilemap.SetTile(pitCell.cellPosition, null);
            HiddenTilemap.SetTile(pitCell.cellPosition, null);
        }
        //Return the pit center so we can add it to the uncovered pit hashtable
        return pitCenter;
    }
}
public class CellWalker
{
    //walker should return a list of Vector3Ints
    //highest age
    Vector3Int currentCell;
    List<Vector3Int> traversedCells;
    int stepsLeft;
    List<Vector3Int> directions;
    public CellWalker(Vector3Int currentCell, int stepsLeft)
    {
        this.currentCell = currentCell;
        this.stepsLeft = stepsLeft;
        traversedCells = new List<Vector3Int> { currentCell};
        directions = new List<Vector3Int>{ 
            new Vector3Int(-1,1,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0),
            new Vector3Int(-1,0,0),                        new Vector3Int(1,0,0),
            new Vector3Int(-1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(1,-1,0)
        };
    }
    public List<Vector3Int> CalculateWalk(List<Vector3Int> invalidCells)
    {
        while(stepsLeft > 0)
        {
            //recalculate valid move choices
            List<Vector3Int> validMoveChoices = new List<Vector3Int>();
            foreach (Vector3Int direction in directions)
            {
                Vector3Int possibleValidChoice = currentCell + direction;
                if(!invalidCells.Contains(possibleValidChoice))
                    validMoveChoices.Add(possibleValidChoice);
            }
            if(validMoveChoices.Count > 0)
            {
                //Then randomly pick a cell
                int moveChoice = Random.Range(0, validMoveChoices.Count);
                currentCell = validMoveChoices[moveChoice];
                traversedCells.Add(currentCell);
            }
            else
            {
                //Debug.LogError("Walker finished walking because no valid choices");
                break;
            }

            stepsLeft--;
        }
        //Debug.LogError("Walker finished walking because steps left = 0");
        return traversedCells;
    }
}