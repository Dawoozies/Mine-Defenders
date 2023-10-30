using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
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

    //Returns all tilemap info
    //GridInformation
    public delegate void GridInteractDelegate(GridInteractArgs args);
    public static event GridInteractDelegate GridInteractEvent;
    //Stone destroyed event
    public delegate void TileDestroyed(Vector3Int cellPos);
    public static event TileDestroyed TileDestroyedEvent;
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
        int durability = gridInformation.GetPositionProperty(cellPos, "Durability", 0);
        durability -= damage;
        if(durability <= 0)
        {
            //Then tile has been destroyed
            TileDestroyedEvent?.Invoke(cellPos);
            gridInformation.SetPositionProperty(cellPos, "Durability", 0);
            return 0;
        }
        gridInformation.SetPositionProperty(cellPos, "Durability", durability);
        return durability;
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
}