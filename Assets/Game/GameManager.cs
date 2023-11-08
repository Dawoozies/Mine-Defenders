using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public class AgentController
    {
        public IAgent player;
        public List<IAgent> enemies;
        public (List<Vector3Int>, List<Vector3Int>) Enemies_PathCalculate()
        {
            if (enemies == null || enemies.Count == 0)
                return (null, null);
            List<Vector3Int> agentCurrentPositionList = new List<Vector3Int>();
            foreach (IAgent agent in enemies)
            {
                agentCurrentPositionList.Add(agent.args.cellPos);
            }
            List<Vector3Int> agentNextStepList = new List<Vector3Int>();
            for (int i = 0;  i < enemies.Count; i++)
            {
                IAgent agent = enemies[i];
                agent.args.path = new List<Vector3Int>();
                //Assume all neighbours are valid to start
                List<Vector3Int> neighbourPositions = GameManager.ins.GetCardinalNeighbourPositions(agent.args.cellPos, false);
                List<Vector3Int> invalidNeighbourPositions = new List<Vector3Int>();
                if (i == 0)
                {
                    //For i == 0 we don't need to check next steps
                    //since this will be the first enemy moving
                    foreach (Vector3Int neighbourPosition in neighbourPositions)
                    {
                        bool neighbourPositionInvalid = false;
                        foreach (Vector3Int item in agentCurrentPositionList)
                        {
                            if(neighbourPosition == item)
                            {
                                neighbourPositionInvalid = true;
                            }
                        }
                        if(neighbourPositionInvalid)
                            invalidNeighbourPositions.Add(neighbourPosition);
                    }
                    if(invalidNeighbourPositions.Count == 4)
                    {
                        //then this agent cannot move next update
                        continue;
                    }
                    agent.args.path =
                        Pathfinding.aStarWithIgnore(
                            agent.args.cellPos,
                            player.args.cellPos,
                            GameManager.ins.GetEnemyInaccessibleTilemaps(),
                            invalidNeighbourPositions
                            );
                    if(agent.args.path != null && agent.args.path.Count > 1)
                    {
                        agentNextStepList.Add(agent.args.path[agent.args.path.Count - 2]);
                        agent.args.pathIndex = agent.args.path.Count - 1;
                    }
                    continue;
                }
                foreach (Vector3Int neighbourPosition in neighbourPositions)
                {
                    bool neighbourPositionInvalid = false;
                    foreach (Vector3Int item in agentCurrentPositionList)
                    {
                        if(neighbourPosition == item)
                        {
                            neighbourPositionInvalid = true;
                        }
                    }
                    foreach (Vector3Int item in agentNextStepList)
                    {
                        if(neighbourPosition == item)
                        {
                            neighbourPositionInvalid = true;
                        }
                    }
                    if(neighbourPositionInvalid)
                        invalidNeighbourPositions.Add(neighbourPosition);
                }
                if(invalidNeighbourPositions.Count == 4)
                {
                    //Then this agent cannot move next update
                    continue;
                }
                agent.args.path =
                    Pathfinding.aStarWithIgnore(
                        agent.args.cellPos,
                        player.args.cellPos,
                        GameManager.ins.GetEnemyInaccessibleTilemaps(),
                        invalidNeighbourPositions
                        );
                if(agent.args.path != null && agent.args.path.Count > 1)
                {
                    agentNextStepList.Add(agent.args.path[agent.args.path.Count - 2]);
                    agent.args.pathIndex = agent.args.path.Count - 1;
                }
                continue;
            }
            return (agentCurrentPositionList, agentNextStepList);
        }
        public void Enemies_MoveToNextStep()
        {
            if (enemies == null || enemies.Count == 0)
                return;
            List<Vector3Int> agentCurrentPositionList = new List<Vector3Int>();
            foreach (IAgent agent in enemies)
            {
                agentCurrentPositionList.Add(agent.args.cellPos);
            }
            List<Vector3Int> agentNextStepList = new List<Vector3Int>();
            for (int i = 0; i < enemies.Count; i++)
            {
                IAgent agent = enemies[i];
                if(agent.args.pathIndex < agent.args.path.Count - 1)
                {
                    int nextIndex = agent.args.pathIndex + 1;
                    Vector3Int nextStepPosition = agent.args.path[nextIndex];
                    if(agentCurrentPositionList.Contains(nextStepPosition))
                    {
                        continue;
                    }
                    if(agentNextStepList.Contains(nextStepPosition))
                    {
                        continue;
                    }
                    agent.args.IncrementProgress(Time.fixedDeltaTime);
                }
            }
        }
    }
    AgentController agentController;
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
        occupiedTiles = new Hashtable();
        reservedTiles = new Hashtable();

        allAgents = new List<IAgent>();
        //player = characterGenerator.CreatePlayer(Vector3Int.zero);
        //Enemy enemyOne = characterGenerator.CreateEnemy(Vector3Int.one);
        //Enemy enemyTwo = characterGenerator.CreateEnemy(-Vector3Int.one);
        agentController = new AgentController();
        agentController.player = characterGenerator.CreatePlayer(Vector3Int.zero);

        agentController.enemies = new List<IAgent>() 
        {
            characterGenerator.CreateEnemy(new Vector3Int(2,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(0,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-2,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(3,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(1,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-1,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-3,3,0)),
        };
    }
    
    public delegate Vector3Int PlayerPositionBufferUpdated(); //Just updates cell position
    public static event PlayerPositionBufferUpdated OnPlayerPositionBufferUpdated;

    public delegate CellData PlayerOccupiedNewCell();
    public static event PlayerOccupiedNewCell OnPlayerOccupiedNewCell;

    public delegate void PlayerOccupiedNewCellUpdated(CellData cellData);
    public static event PlayerOccupiedNewCellUpdated PlayerOccupiedNewCellUpdatedEvent;

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
    public bool computePaths;
    List<Vector3Int> agentCurrentPositionList;
    List<Vector3Int> agentNextStepList;
    public bool moveAlongPaths;
    void Update()
    {
        //This is where we should manage external "coupling"
        if(OnPlayerPositionBufferUpdated != null)
        {
            Vector3Int playerCellPosition = OnPlayerPositionBufferUpdated.Invoke();
            Vector3 playerWorldPosition = CellToWorld(playerCellPosition);
            CameraTrackAnimation(playerWorldPosition);
            OnPlayerPositionBufferUpdated = null;
        }
        if(OnPlayerOccupiedNewCell != null)
        {
            CellData cellData = OnPlayerOccupiedNewCell.Invoke();
            PlayerOccupiedNewCellUpdatedEvent?.Invoke(cellData);

            OnPlayerOccupiedNewCell = null;
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

        if(computePaths)
        {
            (List<Vector3Int>, List<Vector3Int>) PathCalculate_Debug = agentController.Enemies_PathCalculate();
            if(PathCalculate_Debug != (null, null))
            {
                agentCurrentPositionList = PathCalculate_Debug.Item1;
                agentNextStepList = PathCalculate_Debug.Item2;
            }
            computePaths = false;
        }

        if(moveAlongPaths)
        {
            agentController.Enemies_MoveToNextStep();
            moveAlongPaths = false;
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
    public Hashtable occupiedTiles;
    public Hashtable reservedTiles;
    private void OnDrawGizmos()
    {
        Color gizmoColor = Color.black;
        if(agentCurrentPositionList != null && agentCurrentPositionList.Count > 0)
        {
            gizmoColor = Color.white;
            gizmoColor.a = 0.5f;
            Gizmos.color = gizmoColor;
            foreach (var item in agentCurrentPositionList)
            {
                Gizmos.DrawCube(CellToWorld(item), Vector3.one);
            }
        }
        if(agentNextStepList != null && agentNextStepList.Count > 0)
        {
            gizmoColor = Color.blue;
            gizmoColor.a = 0.5f;
            Gizmos.color = gizmoColor;
            foreach (var item in agentNextStepList)
            {
                Gizmos.DrawCube(CellToWorld(item), Vector3.one);
            }
        }
    }
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
    public List<Vector3Int> GetCardinalNeighbourPositions(Vector3Int cell, bool includeSelf)
    {
        List<Vector3Int> neighbourPositions = new List<Vector3Int>();
        foreach (Vector3Int direction in navDirections)
        {
            neighbourPositions.Add(cell + direction);
        }
        if (includeSelf)
            neighbourPositions.Add(cell);
        return neighbourPositions;
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
    public void Update_OccupiedTiles(CellData cellData, IAgent agent)
    {
        bool priorValueExists = occupiedTiles.ContainsValue(agent);
        if (priorValueExists)
        {
            foreach (Vector3Int item in occupiedTiles.Keys)
            {
                if (occupiedTiles[item] != null)
                {
                    if (occupiedTiles[item] == agent)
                        occupiedTiles[item] = null;
                    break;
                }
            }
        }
        Vector3Int key = cellData.cellPosition;
        bool keyExists = occupiedTiles.ContainsKey(key);
        if(keyExists)
        {
            occupiedTiles[key] = agent;
        }
        else
        {
            occupiedTiles.Add(key, agent);
        }
    }
    public void Update_ReservedTiles(CellData cellData, IAgent agent)
    {
        //First we check if there is a reservation prior
        bool priorValueExists = reservedTiles.ContainsValue(agent);
        if(priorValueExists)
        {
            foreach (Vector3Int item in reservedTiles.Keys)
            {
                if (reservedTiles[item] != null)
                {
                    if (reservedTiles[item] == agent)
                        reservedTiles[item] = null;
                    break;
                }
            }
        }
        Vector3Int key = cellData.cellPosition;
        bool keyExists = reservedTiles.ContainsKey(key);
        if (keyExists)
        {
            reservedTiles[key] = agent;
        }
        else
        {
            reservedTiles.Add(key, agent);
        }
    }
    public void Update_EnemyAgentsOnPlayerNewCell(CellData cellData)
    {
        if (allAgents == null || allAgents.Count == 0)
            return;
        foreach (IAgent agent in allAgents)
        {
            agent.navigator.SetNewTarget(cellData);
        }
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
    IAgent occupyingAgent;
    IAgent reservingAgent;
    public List<CellData> GetAllNeighbours(bool includeSelf)
    {
        return GameManager.ins.GetAllNeighboursAroundCell(cellPosition, includeSelf);
    }
    public List<CellData> GetCardinalNeighbours(bool includeSelf)
    {
        return GameManager.ins.GetCardinalNeighboursAroundCell(cellPosition, includeSelf);
    }
    public List<Vector3Int> GetPathToClosestCardinalNeighbour(IAgent agent)
    {
        List<CellData> neighbours = GetCardinalNeighbours(true);
        List<Vector3Int> shortestPath = null;
        foreach (CellData neighbour in neighbours)
        {
            List<Vector3Int> path;
            path = Pathfinding.aStarNew(agent, agent.args.cellPos, neighbour.cellPosition, agent.args.notWalkable, agent.args.reservedTiles);
            if (path == null || path.Count == 0)
                continue;
            if (shortestPath == null)
                shortestPath = path;
            else
                shortestPath = path.Count < shortestPath.Count ? path : shortestPath;
        }
        return shortestPath;
    }
    public List<Vector3Int> GetPathToCell(IAgent agent)
    {
        if(durability > 0)
        {
            return GetPathToClosestCardinalNeighbour((IAgent)agent);
        }
        if(agent.AgentType == AgentType.Enemy)
        {

        }
        List<Vector3Int> path = Pathfinding.aStarNew(agent, agent.args.cellPos, cellPosition, agent.args.notWalkable, agent.args.reservedTiles);
        return path;
    }
    public bool TryOccupation(IAgent agent)
    {
        if(occupyingAgent == null)
        {
            occupyingAgent = agent;
            GameManager.ins.Update_OccupiedTiles(this, agent);
            return true;
        }
        else
        {
            if (reservingAgent == agent)
                return true;
        }
        return false;
    }
    public void ReleaseOccupation(IAgent agent)
    {
        if (occupyingAgent == null)
            return;
        if(agent == occupyingAgent)
        {
            Debug.Log($"Successfully released occupation of {agent.AgentType}");
            occupyingAgent = null;
            GameManager.ins.Update_OccupiedTiles(this, null);
        }
    }
    public bool isOccupiedByOther(IAgent agent)
    {
        return occupyingAgent != null && occupyingAgent != agent;
    }
    public bool TryReservation(IAgent agent)
    {
        if (reservingAgent == null)
        {
            reservingAgent = agent;
            GameManager.ins.Update_ReservedTiles(this, agent);
            return true;
        }
        else
        {
            if (reservingAgent == agent)
            {
                GameManager.ins.Update_ReservedTiles(this, agent);
                return true;
            }
                
        }
        return false;
    }
    public void ReleaseReservation(IAgent releasingAgent)
    {
        if (reservingAgent == null)
            return;
        if(releasingAgent == reservingAgent)
        {
            Debug.Log($"Successfully released occupation of {releasingAgent.AgentType}");
            reservingAgent = null;
            GameManager.ins.Update_ReservedTiles(this, null);
        }
    }
    public bool isReservedByOther(IAgent agent)
    {
        return reservingAgent != null && reservingAgent != agent;
    }
    public bool TurnReserveIntoOccupation(IAgent agent)
    {
        if (occupyingAgent != null && occupyingAgent != agent)
            return false;

        TryOccupation(agent);
        reservingAgent = null;
        return true;
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