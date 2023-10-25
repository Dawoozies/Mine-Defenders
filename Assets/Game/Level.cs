using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//this needs to run first on this scene
public class Level : MonoBehaviour
{
    public Tilemap tilemap;
    GridInformation gridInformation;
    public Tile tile;
    private void Start()
    {
        gridInformation = GetComponent<GridInformation>();
        tilemap.SetTile(new Vector3Int(0, 0, 0), tile);
    }
}
public class TileInformation
{
    public int durability;
}