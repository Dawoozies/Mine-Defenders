using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//This script is THE system which deals with interaction with game world
public class Interaction : MonoBehaviour
{
    public delegate void InteractionHandler(InteractionArgs args);
    public static event InteractionHandler InteractionEvent;

    Camera mainCamera;
    Grid grid;
    GridInformation gridInformation;
    PlayerControls input;

    private void OnEnable()
    {
        mainCamera = Camera.main;
        grid = FindObjectOfType<Grid>();
        gridInformation = grid.GetComponent<GridInformation>();
        input = new PlayerControls();
        input.Player.Tap.performed += (input) =>
        {
            var screenPosition = input.ReadValue<Vector2>();
            var worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;

            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
            InteractionEvent?.Invoke(new InteractionArgs(screenPosition, worldPosition, tilemaps));
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
}
public class InteractionArgs
{
    //A lot of data will be stored here
    public readonly Vector3 screenPosition;
    public readonly Vector3 worldPosition;
    public readonly List<TilemapArgs> tilemapArgsList;
    public InteractionArgs(Vector3 screenPosition, Vector3 worldPosition, Tilemap[] tilemaps)
    {
        this.screenPosition = screenPosition;
        this.worldPosition = worldPosition;

        if (tilemaps == null || tilemaps.Length == 0)
            return;
        tilemapArgsList = new List<TilemapArgs>();
        foreach (Tilemap tilemap in tilemaps)
        {
            tilemapArgsList.Add(new TilemapArgs(tilemap, tilemap.WorldToCell(worldPosition)));
        }
    }
}
public class TilemapArgs
{
    public readonly Tilemap tilemap;
    public readonly Vector3Int cellPosition;
    public readonly Vector3 cellWorldPosition;
    public TilemapArgs(Tilemap tilemap, Vector3Int cellPosition)
    {
        this.tilemap = tilemap;
        this.cellPosition = cellPosition;
        cellWorldPosition = tilemap.GetCellCenterWorld(cellPosition);
    }
}