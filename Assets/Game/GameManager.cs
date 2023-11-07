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
    LootManager lootManager;

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
    //public Transform player;
    public CharacterAgent playerAgent;
    public Vector3Int playerLastCellPos;


    PlayerControls input;
    //Defender character
    //Enemy character
    public Player player;
    public float pitSpawnTime;
    float pitSpawnTimer;
    void Awake()
    {
        ins = this;
        levelGenerator = GetComponentInChildren<LevelGenerator>();
        
        characterGenerator = GetComponentInChildren<CharacterGenerator>();
        lootManager = GetComponentInChildren<LootManager>();
        mainCamera = Camera.main;
        cameraAnimator = mainCamera.GetComponent<ObjectAnimator>();

        //On Tap event
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => 
        {
            Vector3 screenPosition = input.ReadValue<Vector2>();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            Vector3Int cellPosition = WorldToCell(worldPosition);
            Vector3 cellCenterWorldPosition = WorldToCellCenter(worldPosition);
            onTap?.Invoke((CellData)cellTable[cellPosition]);
        };
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    void Start()
    {
        //levelGenerator.ManagedStart();
        cellTable = levelGenerator.GenerateLevelHashtable();
        uncoveredPitCenters = new Hashtable();

        //player = characterGenerator.ManagedStart();
        //playerLastCellPos = WorldToCell(player.position);
        //playerAgent = player.GetComponent<CharacterAgent>();
        //characterGenerator.CreateEnemy();
        reservedTiles = new Hashtable();

        player = characterGenerator.CreatePlayer(Vector3Int.zero);
        allAgents = new List<IAgent>();
    }
    
    public delegate Vector3 PlayerPositionUpdated();
    public static event PlayerPositionUpdated OnPlayerPositionUpdated;

    public delegate void PlayerPositionUpdatedHandler(Vector3Int playerCellPosition, Vector3 playerWorldPosition);
    public static event PlayerPositionUpdatedHandler PlayerPositionUpdatedEvent;

    public delegate void PitUncoveredHandler((Vector3Int, Vector3) pit);
    public static event PitUncoveredHandler PitUncoveredEvent;

    public delegate void OnTap(CellData cellData);
    public static event OnTap onTap;

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
        //Do pit spawning enemies
        if(uncoveredPitCenters != null && uncoveredPitCenters.Keys.Count > 0)
        {
            pitSpawnTimer += Time.deltaTime;
            if(pitSpawnTimer > pitSpawnTime)
            {
                //foreach (Vector3Int pitCenter in uncoveredPitCenters.Keys)
                //{
                //    characterGenerator.CreateEnemy(pitCenter);
                //}
                foreach (Vector3Int item in uncoveredPitCenters.Keys)
                {
                    //Debug.Log($"Spawn enemies at {item}");
                    characterGenerator.CreateEnemy(item);
                }
                pitSpawnTimer = 0;
            }
        }

        return;
        //if(WorldToCell(player.position) != playerLastCellPos)
        //{
        //    playerLastCellPos = WorldToCell(player.position);
        //    PlayerPositionUpdatedEvent?.Invoke(playerLastCellPos, player.position);
        //}
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
    public Hashtable cellTable;
    public Hashtable uncoveredPitCenters;
    public List<IAgent> allAgents;
    public CellData GetCellDataAtPosition(Vector3Int cellPosition)
    {
        return (CellData)cellTable[cellPosition];
    }
    public bool CellTableHasKey(Vector3Int cellPosition)
    {
        return cellTable.ContainsKey(cellPosition);
    }
    public void SetCellDataAtPosition(Vector3Int cellPosition, CellData cellData)
    {
        cellTable.Add(cellPosition, cellData);
    }
    public List<CellData> GetAllNeighboursAroundCell(Vector3Int cell, bool includeSelf)
    {
        List<CellData> neighbours = new List<CellData>();
        foreach (Vector3Int direction in directions)
        {
            CellData neighbourData = GetCellDataAtPosition(cell + direction);
            neighbours.Add(neighbourData);
        }
        if (includeSelf)
            neighbours.Add(GetCellDataAtPosition(cell));
        return neighbours;
    }
    public List<CellData> GetCardinalNeighboursAroundCell(Vector3Int cell, bool includeSelf)
    {
        List<CellData> neighbours = new List<CellData>();
        foreach (Vector3Int direction in navDirections)
        {
            CellData neighbourData = GetCellDataAtPosition(cell + direction);
            neighbours.Add(neighbourData);
        }
        if (includeSelf)
            neighbours.Add(GetCellDataAtPosition(cell));
        return neighbours;
    }
    public List<CellData> GetAllNeighboursAroundCell_InGivenHashtable(Hashtable givenHashtable, Vector3Int cell, bool includeSelf)
    {
        List<CellData> neighbours = new List<CellData>();
        foreach (Vector3Int direction in directions)
        {
            CellData neighbourData = (CellData)givenHashtable[cell + direction];
            neighbours.Add(neighbourData);
        }
        if (includeSelf)
            neighbours.Add((CellData)givenHashtable[cell]);
        return neighbours;
    }
    public List<CellData> GetCardinalNeighboursAroundCell_InGivenHashtable(Hashtable givenHashtable, Vector3Int cell, bool includeSelf)
    {
        List<CellData> neighbours = new List<CellData>();
        foreach (Vector3Int direction in navDirections)
        {
            CellData neighbourData = (CellData)givenHashtable[cell + direction];
            neighbours.Add(neighbourData);
        }
        if (includeSelf)
            neighbours.Add((CellData)givenHashtable[cell]);
        return neighbours;
    }
    public void BreakStone(CellData cellData)
    {
        //Remove stone via level generator method
        //call method on loot manager for ores
        levelGenerator.RemoveStoneTileAtCell(cellData.cellPosition);
        levelGenerator.GenerateFloor(cellData.cellPosition);
        Debug.Log("stone break");
        if (cellData.isPit && !cellData.isUncoveredPit)
        {
            //Pit uncovered here
            CellData pitCenter = levelGenerator.UncoverFullPit(cellData);
            if (uncoveredPitCenters.ContainsKey(pitCenter.cellPosition))
            {
                uncoveredPitCenters[pitCenter.cellPosition] = pitCenter;
            }
            else
            {
                uncoveredPitCenters.Add(pitCenter.cellPosition, pitCenter);
            }
            return;
        }
        //Dont spawn ore if its a pit uncovering
        if (cellData.ore != null)
            cellData.loot = lootManager.InstantiateLoot_Ore(cellData.cellCenterWorldPosition, cellData.ore);
    }
}
public class CellData
{
    public Vector3Int cellPosition;
    public Vector3 cellCenterWorldPosition;
    public bool isPlayerSpawnArea;
    public bool isLevelBoundary;
    public Ore ore;
    public int durability;
    public bool isPit;
    public bool isUncoveredPit;
    public bool isPitCenter;
    public CellLoot loot;
    public List<CellData> GetAllNeighbours(bool includeSelf)
    {
        return GameManager.ins.GetAllNeighboursAroundCell(cellPosition, includeSelf);
    }
    public List<CellData> GetCardinalNeighbours(bool includeSelf)
    {
        return GameManager.ins.GetCardinalNeighboursAroundCell(cellPosition, includeSelf);
    }
    public List<Vector3Int> GetPathToClosestCardinalNeighbour(Vector3Int point, Tilemap[] notWalkable, Hashtable reservedTiles)
    {
        List<CellData> neighbours = GetCardinalNeighbours(true);
        List<Vector3Int> shortestPath = null;
        foreach (CellData neighbour in neighbours)
        {
            List<Vector3Int> path;
            path = Pathfinding.aStarNew(point, neighbour.cellPosition, notWalkable, reservedTiles);
            if (path == null || path.Count == 0)
                continue;
            if (shortestPath == null)
                shortestPath = path;
            else
                shortestPath = path.Count < shortestPath.Count ? path : shortestPath;
        }
        return shortestPath;
    }
    public void CellInfoDebug()
    {
        Debug.Log("[--------------------------------]");
        Debug.Log($"cellPosition = {cellPosition}");
        Debug.Log($"cellCenterWorldPosition = {cellCenterWorldPosition}");
        Debug.Log($"isPlayerSpawnArea = {cellPosition}");
        Debug.Log($"isLevelBoundary = {cellCenterWorldPosition}");
        string oreDebug = ore == null ? "ore = null" : $"ore = {ore.name}";
        Debug.Log(oreDebug);
        Debug.Log($"durability = {durability}");
        Debug.Log($"isPit = {isPit}");
        Debug.Log($"isUncoveredPit = {isUncoveredPit}");
        Debug.Log("[--------------------------------]");
    }
}
[Flags]
public enum CellContents
{
    None = 0,
    Ore = 1,
    Stone = 2,
}