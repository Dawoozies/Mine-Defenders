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
        //tilemap.SetTile(new Vector3Int(0, 0, 0), tile);
        //gridInformation.SetPositionProperty<TileInformation>(new Vector3Int(0,0,0), "TileInformation", new TileInformation(3));
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