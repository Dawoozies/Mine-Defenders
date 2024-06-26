using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridInteraction : MonoBehaviour
{
    PlayerControls input;
    Camera mainCamera;
    Grid grid;
    GridInformation gridInformation;
    Tilemap[] tilemaps;
    LevelGenerator levelGenerator;

    List<Vector3Int> navDirections = new List<Vector3Int>{
                                     new Vector3Int(0,1,0),
            new Vector3Int(-1,0,0),                        new Vector3Int(1,0,0),
                                     new Vector3Int(0,-1,0),
    };

    //Returns all tilemap info
    //GridInformation
    public delegate void GridInteractDelegate(GridInteractArgs args);
    public static event GridInteractDelegate GridInteractEvent;
    //Stone destroyed event
    public delegate void StoneDestroyed(StoneDestroyedArgs args);
    public static event StoneDestroyed StoneDestroyedEvent;

    public delegate (Vector3Int, int) DamageDurability();
    public static event DamageDurability OnDamageDurability;
    private void OnEnable()
    {
        mainCamera = Camera.main;
        grid = GetComponent<Grid>();
        gridInformation = grid.GetComponent<GridInformation>();
        tilemaps = grid.GetComponentsInChildren<Tilemap>();
        levelGenerator = GetComponent<LevelGenerator>();
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            Vector3 screenInputPos = input.ReadValue<Vector2>();
            Vector3 worldInputPos = mainCamera.ScreenToWorldPoint(screenInputPos);
            worldInputPos.z = 0;

            Vector3Int cellPos = tilemaps[0].WorldToCell(worldInputPos);
            //Package grid interact args
            GridInteractArgs args = new GridInteractArgs();
            args.ScreenPosition = screenInputPos;
            args.WorldPosition = worldInputPos;
            args.CellPosition = cellPos;
            args.CellWorldPosition = tilemaps[0].GetCellCenterWorld(cellPos);
            args.durability = GetDurabilityAtCell(cellPos);
            args.isWalkable = 
                !levelGenerator.StoneTilemap.HasTile(cellPos) &&
                !levelGenerator.PitTilemap.HasTile(cellPos) &&
                levelGenerator.FloorTilemap.HasTile(cellPos);

            args.walkableNeighbours = new List<(Vector3Int, Vector3)>();

            //stone and pit not walkable
            foreach (Vector3Int direction in navDirections)
            {
                Vector3Int neighbourCellPos = cellPos + direction;
                TileBase stoneTile = levelGenerator.StoneTilemap.GetTile(neighbourCellPos);
                TileBase pitTile = levelGenerator.PitTilemap.GetTile(neighbourCellPos);
                TileBase floorTile = levelGenerator.FloorTilemap.GetTile(neighbourCellPos);
                if (stoneTile == null && pitTile == null && floorTile != null)
                {
                    //then tile is walkable
                    //packaging CellPos + CellCenterWorldPos
                    args.walkableNeighbours.Add((neighbourCellPos, tilemaps[0].GetCellCenterWorld(neighbourCellPos)));
                    //Debug.Log($"Neighbour to {cellPos} at {neighbourCellPos} is walkable");
                }
            }

            GridInteractEvent?.Invoke(args);

            //DebugGridInteraction(cellPos);

            //foreach (Tilemap tilemap in tilemaps)
            //{
            //    Vector3Int cellPos = tilemap.WorldToCell(worldInputPos);
            //    tilemap.SetTile(cellPos, null);
            //    if(tilemap.name == "Floor")
            //    {
            //        levelGenerator.GenerateFloor(cellPos);
            //    }
            //}
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void DebugGridInteraction(Vector3Int cellPos)
    {
        string oreName = gridInformation.GetPositionProperty(cellPos, "OreName", "No Ore");
        int durability = gridInformation.GetPositionProperty(cellPos, "Durability", 0);
        Debug.Log("[=========== GRID INTERACTION] ===========]");
        Debug.Log($"cellPos = {cellPos}");
        Debug.Log($"oreName = {oreName}");
        Debug.Log($"durability = {durability}");
        Debug.Log("[=========== GRID INTERACTION] ===========]");
    }
    int GetDurabilityAtCell(Vector3Int cellPos)
    {
        int durability = gridInformation.GetPositionProperty(cellPos, "Durability", 0);
        return durability;
    }
    string GetOreNameAtCell(Vector3Int cellPos)
    {
        string oreName = gridInformation.GetPositionProperty(cellPos, "OreName", "No Ore");
        return oreName;
    }
    int DamageDurabilityAtCell(Vector3Int cellPos, int damage)
    {
        int durability = GetDurabilityAtCell(cellPos);
        durability -= damage;
        if(durability <= 0)
        {
            gridInformation.SetPositionProperty(cellPos, "Durability", 0);
            if(GetOreNameAtCell(cellPos) != "No Ore")
                gridInformation.SetPositionProperty(cellPos, "OreName", "No Ore");

            for (int i = 0; i < tilemaps.Length - 1; i++)
            {
                tilemaps[i].SetTile(cellPos, null);
            }

            //Do after we have removed the tile
            StoneDestroyedArgs args = new StoneDestroyedArgs();
            args.CellPosition = cellPos;
            args.CellWorldPosition = GameManager.ins.CellToWorld(cellPos);
            args.ore = GetOreNameAtCell(cellPos);
            StoneDestroyedEvent?.Invoke(args);

            return 0;
        }
        gridInformation.SetPositionProperty(cellPos, "Durability", durability);
        //Debug.Log($"CellPos={cellPos} DealtDamage={damage} DurabilityLeft={durability}");
        return durability;
    }
    private void Update()
    {
        if(OnDamageDurability != null)
        {
            (Vector3Int, int) args = OnDamageDurability.Invoke();
            DamageDurabilityAtCell(args.Item1, args.Item2);
            OnDamageDurability = null;
        }
    }
}
public class GridInteractArgs
{
    public Vector3 ScreenPosition;
    public Vector3 WorldPosition;
    public Vector3Int CellPosition;
    public Vector3 CellWorldPosition;
    public string oreName;
    public int durability;
    public bool isWalkable;
    public List<(Vector3Int, Vector3)> walkableNeighbours;
}
public class StoneDestroyedArgs
{
    public Vector3Int CellPosition;
    public Vector3 CellWorldPosition;
    public string ore;
}