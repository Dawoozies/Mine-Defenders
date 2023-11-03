using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
public class GameManager : MonoBehaviour
{
    public static GameManager ins;

    LevelGenerator levelGenerator;
    CharacterGenerator characterGenerator;
    PitManager pitManager;

    Camera mainCamera;
    ObjectAnimator cameraAnimator;

    GridInformation gridInformation;

    public List<Vector3Int> directions = new List<Vector3Int>{
            new Vector3Int(-1,1,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0),
            new Vector3Int(-1,0,0),                        new Vector3Int(1,0,0),
            new Vector3Int(-1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(1,-1,0)
    };

    public List<Vector3Int> navDirections = new List<Vector3Int>{
                                     new Vector3Int(0,1,0),
            new Vector3Int(-1,0,0),                        new Vector3Int(1,0,0),
                                     new Vector3Int(0,-1,0),
    };

    //Player character
    public Transform player;
    public CharacterAgent playerAgent;
    public Vector3Int playerLastCellPos;

    //Defender character
    //Enemy character
    void Awake()
    {
        ins = this;
        levelGenerator = GetComponentInChildren<LevelGenerator>();
        characterGenerator = GetComponentInChildren<CharacterGenerator>();
        pitManager = GetComponentInChildren<PitManager>();
        mainCamera = Camera.main;
        cameraAnimator = mainCamera.GetComponent<ObjectAnimator>();

        gridInformation = GetComponentInChildren<GridInformation>();
    }
    void Start()
    {
        levelGenerator.ManagedStart();

        player = characterGenerator.ManagedStart();
        playerLastCellPos = WorldToCell(player.position);
        playerAgent = player.GetComponent<CharacterAgent>();
        pitManager.ManagedStart();
        //characterGenerator.CreateEnemy();
        reservedTiles = new Hashtable();
    }
    
    public delegate Vector3 PlayerPositionUpdated();
    public static event PlayerPositionUpdated OnPlayerPositionUpdated;

    public delegate void PlayerPositionUpdatedHandler(Vector3Int playerCellPosition, Vector3 playerWorldPosition);
    public static event PlayerPositionUpdatedHandler PlayerPositionUpdatedEvent;

    public delegate void PitUncoveredHandler((Vector3Int, Vector3) pit);
    public static event PitUncoveredHandler PitUncoveredEvent;

    public bool isInLevelBounds(Vector3Int position) {
        if (position.x < levelGenerator.bottomLeftCorner.x || position.x > levelGenerator.topRightCorner.x)
        {
            return false;
        }
        if (position.y < levelGenerator.bottomLeftCorner.y || position.y > levelGenerator.topRightCorner.y)
        {
            return false;
        }
        return true;
    }
    void Update()
    {
        //This is where we should manage external "coupling"
        if(OnPlayerPositionUpdated != null)
        {
            Vector3 PlayerPosition = OnPlayerPositionUpdated.Invoke();
            CameraTrackAnimation(PlayerPosition);
            OnPlayerPositionUpdated = null;
        }

        if(WorldToCell(player.position) != playerLastCellPos)
        {
            playerLastCellPos = WorldToCell(player.position);
            PlayerPositionUpdatedEvent?.Invoke(playerLastCellPos, player.position);
        }
    }
    
    void CameraTrackAnimation(Vector3 targetPos)
    {
        targetPos.z = mainCamera.transform.position.z;
        ObjectAnimation trackingAnimation = new ObjectAnimation();
        trackingAnimation.animName = "PlayerTracking";
        trackingAnimation.frames = 2;
        trackingAnimation.positions = new List<Vector3> { mainCamera.transform.position, targetPos};
        trackingAnimation.interpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp};
        trackingAnimation.loop = false;

        cameraAnimator.CreateAndPlayAnimation(trackingAnimation);
    }
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        worldPosition.z = 0;
        return levelGenerator.StoneTilemap.WorldToCell(worldPosition);
    }
    public Vector3 WorldToCellCenter(Vector3 worldPosition)
    {
        worldPosition.z = 0;
        return levelGenerator.StoneTilemap.GetCellCenterWorld(WorldToCell(worldPosition));
    }
    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return levelGenerator.StoneTilemap.GetCellCenterWorld(cellPos);
    }

    public Tilemap[] GetPlayerInaccessibleTilemaps() 
    { 
        List<Tilemap> environments = new List<Tilemap>();
        environments.Add(levelGenerator.StoneTilemap);
        environments.Add(levelGenerator.PitTilemap);
        return environments.ToArray();
    }

    public Tilemap[] GetEnemyInaccessibleTilemaps()
    {
        List<Tilemap> environments = new List<Tilemap>();
        environments.Add(levelGenerator.StoneTilemap);
        return environments.ToArray();
    }

    public void UncoverPit(Vector3Int pitCenter)
    {
        (Vector3Int, Vector3) pit = (pitCenter, CellToWorld(pitCenter));
        PitUncoveredEvent?.Invoke(pit);
    }
    public bool IsUncoveredPit(Vector3Int cellPos)
    {
        return 
            gridInformation.GetPositionProperty(cellPos, "IsPit", 0) == 1
            && gridInformation.GetPositionProperty(cellPos, "IsUncoveredPit", 0) == 1;
    }
    public bool SpawnEnemy(Vector3 spawnPosition)
    {
        if (!CanEnemySpawnHere(spawnPosition)){
            return false;
        }
        characterGenerator.CreateEnemy(spawnPosition);
        return true;
    }

    public bool CanEnemySpawnHere(Vector3 spawnPosition) 
    {
        foreach (CharacterAgent agent in getEnemyAgents())
        {
            if (Vector3.Distance(agent.transform.position, spawnPosition) < 1.2f) 
            {
                return false;
            }
        }

        return true;
    }

    public List<CharacterAgent> getEnemyAgents() {
        return characterGenerator.enemyAgents;
    }

    public Hashtable reservedTiles;
    public bool TryReserve(Vector3Int cellPos)
    {
        if(reservedTiles.ContainsKey(cellPos))
        {
            return false;
        }

        reservedTiles.Add(cellPos, 1);
        return true;
    }
    public void ReleaseReservation(Vector3Int cellPos)
    {
        if (reservedTiles.ContainsKey(cellPos))
        {
            reservedTiles.Remove(cellPos);
        }
    }
}
