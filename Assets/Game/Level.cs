using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//this needs to run first on this scene
public class Level : MonoBehaviour
{
    public Tilemap OreTilemap;
    public Tilemap StoneTilemap;
    public Tilemap SolidColorTilemap;
    GridInformation gridInformation;
    public RuleTile ore;
    public RuleTile stone;
    public Tile wallSolidColor;
    public Vector2Int bottomLeftCorner;
    public Vector2Int topRightCorner;
    public Vector2Int spawnAreaBottomLeftCorner;
    public Vector2Int spawnAreaTopRightCorner;
    public Ore emptyOre;
    public List<Ore> ores;
    private void Start()
    {
        gridInformation = GetComponent<GridInformation>();
        //tilemap.SetTile(new Vector3Int(0, 0, 0), tile);
        //gridInformation.SetPositionProperty<TileInformation>(new Vector3Int(0,0,0), "TileInformation", new TileInformation(3));
        //64
        //
        //tilemap.BoxFill(new Vector3Int(0, 0, 0), null, 0, 0, 4, 4);
        GenerateLevel();
    }
    void GenerateLevel()
    {
        //Tilemaps which are not shifted by negative of their tile anchors will be off center
        for (int x = bottomLeftCorner.x; x <= topRightCorner.x; x++)
        {
            for(int y  = bottomLeftCorner.y; y <= topRightCorner.y; y++)
            {
                var currentCoordinate = new Vector3Int(x,y,0);
                if(spawnAreaBottomLeftCorner.x < x && x < spawnAreaTopRightCorner.x && spawnAreaBottomLeftCorner.y < y && y < spawnAreaTopRightCorner.y)
                {
                    //Do nothing
                }
                else
                {
                    Ore highestRarityOreRolled = null;
                    foreach (Ore ore in ores)
                    {
                        if(ore.BaseRoll())
                        {
                            //Then ore roll successful
                            if (highestRarityOreRolled == null || ore.rarity > highestRarityOreRolled.rarity)
                            {
                                highestRarityOreRolled = ore;
                                continue;
                            }
                        }
                    }

                    if(highestRarityOreRolled != null)
                    {
                        OreTilemap.SetTile(currentCoordinate, ore);
                        OreTilemap.SetColor(currentCoordinate, highestRarityOreRolled.color);
                        //Make sure to set the ore properties
                        //Like durability
                    }
                    else
                    {
                        OreTilemap.SetTile(currentCoordinate, ore);
                        OreTilemap.SetColor(currentCoordinate, emptyOre.color);
                    }
                    StoneTilemap.SetTile(currentCoordinate, stone);
                    SolidColorTilemap.SetTile(currentCoordinate, wallSolidColor);
                }
            }
        }
    }
}
public class GridData
{
    //define what is on the ground layer
    //
    //determine whether it is boundary wall or not
    public int durability;
    public GridData(int durability)
    {
        this.durability = durability;
    }
}