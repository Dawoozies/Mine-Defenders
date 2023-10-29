using System.Collections;
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
    private void OnEnable()
    {
        mainCamera = Camera.main;
        grid = GetComponent<Grid>();
        gridInformation = grid.GetComponent<GridInformation>();
        tilemaps = grid.GetComponentsInChildren<Tilemap>();
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            Vector3 screenInputPos = input.ReadValue<Vector2>();
            Vector3 worldInputPos = mainCamera.ScreenToWorldPoint(screenInputPos);
            worldInputPos.z = 0;

            foreach (Tilemap tilemap in tilemaps)
            {
                Vector3Int cellPos = tilemap.WorldToCell(worldInputPos);
                tilemap.SetTile(cellPos, null);
            }
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
