using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
//Player interaction with world
public class PlayerInteraction : MonoBehaviour
{
    PlayerControls input;
    Camera MainCamera;
    Vector3 ScreenPosition;
    Vector3 WorldPosition;

    //Player
    Player Player;

    //Destructible Environment
    Destructible Destructible;
    Tilemap DestructibleTilemap;
    Vector3Int DestructibleCellPosition;
    private void OnEnable()
    {
        MainCamera = Camera.main;
        //Player
        Player = FindAnyObjectByType<Player>();
        //Destructible
        Destructible = FindAnyObjectByType<Destructible>();
        DestructibleTilemap = Destructible.GetComponent<Tilemap>();

        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            ScreenPosition = input.ReadValue<Vector2>();
            WorldPosition = MainCamera.ScreenToWorldPoint(ScreenPosition);
            WorldPosition.z = 0;

            //ping the destructible environment
            DestructibleCellPosition = DestructibleTilemap.WorldToCell(WorldPosition);
            TileBase destructibleTile = DestructibleTilemap.GetTile(DestructibleCellPosition);
            if(destructibleTile != null)
            {
                currentState = InteractState.Mining;
                Player.StartMining(DestructibleTilemap, DestructibleCellPosition, destructibleTile);
            }
            else
            {
                currentState = InteractState.Moving;
                Player.ForceStopMining();
            }
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    //Debug stuff
    InteractState currentState;
    public TextMeshProUGUI interactDebugText;
    void Update()
    {
        interactDebugText.text = currentState.ToString();
    }
}
public enum InteractState
{
    Idle = 0,
    Moving = 1,
    Mining = 2,
}