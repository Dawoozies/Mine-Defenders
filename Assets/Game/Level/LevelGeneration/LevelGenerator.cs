using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public Tilemap OreTilemap;
    public Tilemap StoneTilemap;
    public Tilemap StoneColorTilemap;
    public Tilemap HiddenTilemap;
    public Tilemap FloorTilemap;
    List<Vector3Int> ignoreCells;

    public Vector2Int bottomLeftCorner;
    public Vector2Int topRightCorner;
    public Vector2Int spawnAreaBottomLeftCorner;
    public Vector2Int spawnAreaTopRightCorner;

    public RuleTile hiddenTile;
    public RuleTile floorTile;
    public RuleTile ore;
    public RuleTile stone;
    public RuleTile stoneColor;
    public Color defaultStoneColor;

    public Ore emptyOre;
    public List<Ore> ores;

    GridInformation GridInformation;
    public void ManagedStart()
    {
        GridInformation = GetComponent<GridInformation>();

        ignoreCells = new List<Vector3Int>();
        List<Vector3Int> levelBoundary = Boundaries(bottomLeftCorner - Vector2Int.one, topRightCorner + Vector2Int.one);
        List<Vector3Int> spawnBoundary = Boundaries(spawnAreaBottomLeftCorner + Vector2Int.one, spawnAreaTopRightCorner - Vector2Int.one);
        ignoreCells.AddRange(levelBoundary);
        ignoreCells.AddRange(spawnBoundary);

        GenerationLayer_StoneAndSeedOreWalkers();

        GridInteraction.StoneDestroyedEvent += (StoneDestroyedArgs args) => { GenerateFloor(args.CellPosition); };
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
    void GenerationLayer_StoneAndSeedOreWalkers()
    {
        //Generates stone and does seed ore walkers
        //Once an ore walker has been put down
        //add cell to ignore list and do random walks
        for (int x = bottomLeftCorner.x; x <= topRightCorner.x; x++)
        {
            for(int y = bottomLeftCorner.y; y <= topRightCorner.y; y++)
            {
                Vector3Int currentCoordinate = new Vector3Int(x, y, 0);

                //Ignore generation if in spawn area or current coordinates are on the ignore list
                bool inSpawnArea =
                    (spawnAreaBottomLeftCorner.x < x && x < spawnAreaTopRightCorner.x &&
                    spawnAreaBottomLeftCorner.y < y && y < spawnAreaTopRightCorner.y);
                bool ignoreCell = inSpawnArea || ignoreCells.Contains(currentCoordinate);

                if (!ignoreCell)
                {
                    //StoneTilemap.SetTile(currentCoordinate, stone);
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
                        //Debug.LogError($"Starting random walk for: {highestRarityOreRolled.name} with steps = {stepsRoll}");
                        CellWalker oreWalker = new CellWalker(currentCoordinate, stepsRoll);
                        List<Vector3Int> walkedTiles = oreWalker.CalculateWalk(ignoreCells);
                        ignoreCells.AddRange(walkedTiles);
                        foreach (Vector3Int walkedTile in walkedTiles)
                        {
                            OreTilemap.SetTile(walkedTile, ore);
                            OreTilemap.SetColor(walkedTile, highestRarityOreRolled.color);

                            GridInformation.SetPositionProperty(walkedTile, "OreName", highestRarityOreRolled.name);
                            GridInformation.SetPositionProperty(walkedTile, "Durability", highestRarityOreRolled.durability);
                        }
                    }
                    else
                    {
                        OreTilemap.SetTile(currentCoordinate, ore);
                        OreTilemap.SetColor(currentCoordinate, emptyOre.color);
                    }

                }
                if(!inSpawnArea)
                {
                    StoneTilemap.SetTile(currentCoordinate, stone);
                    StoneColorTilemap.SetTile(currentCoordinate, stoneColor);
                    StoneColorTilemap.SetColor(currentCoordinate, defaultStoneColor);
                    HiddenTilemap.SetTile(currentCoordinate, hiddenTile);

                    int existingDurability = GridInformation.GetPositionProperty(currentCoordinate, "Durability", 0);
                    GridInformation.SetPositionProperty(currentCoordinate, "Durability", existingDurability + 2);
                }
                if(inSpawnArea)
                {
                    GenerateFloor(currentCoordinate);
                }

                
            }
        }
    }
    public void GenerateFloor(Vector3Int cellPos)
    {
        FloorTilemap.SetTile(cellPos, floorTile);
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