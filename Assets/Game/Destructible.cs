using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class Destructible : MonoBehaviour
{
    NavMeshData navMeshData;
    NavMeshSurface Surface2D;
    PlayerControls input;
    Tilemap tilemap;
    Camera mainCamera;
    private void OnEnable()
    {
        mainCamera = Camera.main;
        tilemap = GetComponent<Tilemap>();
        //Should only be one of these, be careful with this
        Surface2D = FindAnyObjectByType<NavMeshSurface>();
        navMeshData = Surface2D.navMeshData;
        
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            var touchPos = input.ReadValue<Vector2>();
            var worldPos = mainCamera.ScreenToWorldPoint(touchPos);
            //tilemaps are 3d so make sure we set the z=-10 we get from the transform to z=0
            worldPos.z = 0;
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            Debug.Log(cellPos);
            tilemap.SetTile(cellPos, null);
            //Only refresh tiles which are actually affected
            //dont use refresh all tiles for the love of god
            tilemap.RefreshTile(cellPos);

            //Update the nav mesh now
            Surface2D.UpdateNavMesh(navMeshData);
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void Update()
    {
        
    }
}
