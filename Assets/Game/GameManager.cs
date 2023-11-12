using System;
using System.Collections;
using System.Collections.Generic;
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
        public Player player;
        public IAgent playerAgent;
        public List<IAgent> enemies;
        public void Player_PathCalculate(CellData cellData)
        {
            playerAgent.args.ResetCompletedFullPathEvent();
            bool targetingStone = cellData.durability > 0;
            if(targetingStone)
            {
                #region Set up pickaxe animation stuff
                player.tool.toolTargetCellPos = cellData.cellPosition;
                player.tool.toolTargetCellCenterWorldPos = cellData.cellCenterWorldPosition;
                #endregion
                List<Vector3Int> shortestPathToNeighbour =
                    cellData.GetPathToClosestCardinalNeighbour(player);
                #region Case : Where we are right next to the stone we targeted
                if (Vector3Int.Distance(playerAgent.args.cellPos, cellData.cellPosition) == 1)
                {
                    player.StartMiningAnimation();
                    //then we can just start mining
                }
                #endregion
                if (shortestPathToNeighbour == null)
                    return;
                #region Case : Where we have to path to the stone
                #region Set up playerFullPath
                List<AgentPath> fullPath = new List<AgentPath>();
                for (int i = shortestPathToNeighbour.Count - 1; i > 0; i--)
                {
                    var path = new AgentPath(
                        shortestPathToNeighbour[i], 
                        shortestPathToNeighbour[i - 1], 
                        playerAgent.args.moveInterpolationType);
                    fullPath.Add(path);
                }
                playerAgent.args.playerPath = fullPath;
                playerAgent.args.playerPathIndex = 0;
                #endregion
                playerAgent.args.onPlayerCompletedFullPath += player.StartMiningAnimation;
                #endregion
            }
            else
            {
                List<Vector3Int> points = Pathfinding.aStarNew(
                    playerAgent, 
                    playerAgent.args.cellPos, 
                    cellData.cellPosition, 
                    GameManager.ins.GetPlayerInaccessibleTilemaps(), 
                    null
                    );

                if (points == null || points.Count == 0)
                    return;
                #region Set up playerFullPath
                List<AgentPath> fullPath = new List<AgentPath>();
                for (int i = points.Count - 1; i > 0; i--)
                {
                    var path = new AgentPath(
                        points[i],
                        points[i - 1],
                        playerAgent.args.moveInterpolationType);
                    fullPath.Add(path);
                }
                playerAgent.args.playerPath = fullPath;
                playerAgent.args.playerPathIndex = 0;
                #endregion
            }
        }
        public void Enemies_PathCalculate()
        {
            if (enemies == null || enemies.Count == 0)
                return;

            #region CurrentPositions + Clear NextStep List
            List<Vector3Int> agentCurrentPositionList = new List<Vector3Int>();
            List<Vector3Int> agentNextStepList = new List<Vector3Int>();
            foreach (IAgent agent in enemies)
            {
                agentCurrentPositionList.Add(agent.args.cellPos);
                if(agent.args.path != null)
                {
                    agentNextStepList.Add(agent.args.path.end);
                }
            }
            #endregion
            for (int i = 0;  i < enemies.Count; i++)
            {
                IAgent agent = enemies[i];
                //path to write to
                if (agent.args.hasInstruction)
                    continue;
                if (agent.args.movesLeft <= 0)
                    continue;
                AgentPath path = new AgentPath(agent.args.cellPos, agent.args.cellPos, agent.args.moveInterpolationType);
                agent.args.path = path;
                List<Vector3Int> points = new List<Vector3Int>();
                #region Calculate Neighbour Positions To Ignore
                //Assume all neighbours are valid to start
                List<Vector3Int> neighbourPositions = GameManager.ins.GetCardinalNeighbourPositions(agent.args.cellPos, false);
                List<Vector3Int> invalidNeighbourPositions = new List<Vector3Int>();
                foreach (Vector3Int neighbourPosition in neighbourPositions)
                {
                    bool neighbourPositionInvalid = false;
                    foreach (Vector3Int item in agentCurrentPositionList)
                    {
                        if(neighbourPosition == item && item != agent.args.cellPos)
                        {
                            neighbourPositionInvalid = true;
                        }
                    }
                    if (i != 0)
                    {
                        foreach (Vector3Int item in agentNextStepList)
                        {
                            if (neighbourPosition == item)
                            {
                                neighbourPositionInvalid = true;
                            }
                        }
                    }
                    if (agent.args.previousPoint.z != -1)
                    {
                        if (neighbourPosition == agent.args.previousPoint)
                        {
                            neighbourPositionInvalid = true;
                            agent.args.previousPoint = new Vector3Int(0, 0, -1);
                        }
                    }
                    if (neighbourPositionInvalid)
                        invalidNeighbourPositions.Add(neighbourPosition);
                }
                #endregion
                if (invalidNeighbourPositions.Count == 4)
                {
                    //Then this agent cannot move next update
                    continue;
                }
                #region Call Pathfinding.aStartWithIgnore
                points =
                    Pathfinding.aStarWithIgnore(
                        agent.args.cellPos,
                        playerAgent.args.cellPos,
                        GameManager.ins.GetEnemyInaccessibleTilemaps(),
                        invalidNeighbourPositions
                        );
                #endregion
                #region Setup Non Trivial End For Path
                if (points != null && points.Count > 1)
                {
                    if (points[points.Count - 2] != playerAgent.args.cellPos)
                    {
                        agentNextStepList.Add(points[points.Count - 2]);
                        path.end = points[points.Count - 2];
                        if (!agent.args.hasInstruction)
                            agent.args.hasInstruction = true;
                        continue;
                    }
                }
                #endregion
            }
        }
        public void Agents_RefreshMovesLeft()
        {
            foreach (IAgent agent in enemies)
            {
                agent.args.RefreshMovesLeft();
            }
        }
        public bool CheckAgentCanSpawnAtPosition(Vector3Int spawnPosition)
        {
            bool canSpawnHere = true;
            foreach (IAgent agent in enemies)
            {
                if (agent.args.cellPos == spawnPosition)
                {
                    canSpawnHere = false;
                }
            }
            return canSpawnHere;
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
        agentController = new AgentController();
        agentController.player = characterGenerator.CreatePlayer(Vector3Int.zero);
        agentController.playerAgent = agentController.player;

        agentController.playerAgent.args.onPlayerNoMovesLeft += agentController.Agents_RefreshMovesLeft;

        onTap += agentController.Player_PathCalculate;

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
                    if(agentController.CheckAgentCanSpawnAtPosition(item))
                    {
                        Enemy newEnemy = characterGenerator.CreateEnemy(item);
                        agentController.enemies.Add(newEnemy);
                    }
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
    private void FixedUpdate()
    {
        agentController.playerAgent.args.MoveAlongPath(Time.fixedDeltaTime);
        foreach (IAgent agent in agentController.enemies)
        {
            //Debug.Log($"agent.args.hasInstruction = {agent.args.hasInstruction}");
            //Debug.Log($"agent.args.movesLeft = {agent.args.movesLeft}");
            //Debug.Log($"agent.args.path == null = {agent.args.path == null}");
            if (agent.args.hasInstruction)
            {
                agent.args.MoveAlongPath(Time.fixedDeltaTime);
            }
        }
        agentController.Enemies_PathCalculate();
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
    public Hashtable occupiedTiles;
    public Hashtable reservedTiles;
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Color gizmoColor = Color.black;
        foreach (IAgent agent in agentController.enemies)
        {
            if(!agent.args.hasInstruction)
            {
                gizmoColor = Color.black;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(agent.args.cellPos, Vector3.one);
                continue;
            }
            if (agent.args.path != null && !agent.args.path.completed)
            {
                gizmoColor = Color.red;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
            }
            if (agent.args.path != null && agent.args.path.completed)
            {
                gizmoColor = Color.green;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
            }
            Gizmos.DrawWireCube(agent.args.worldPos, Vector3.one);
            if (agent.args.previousPoint.z != -1)
            {
                gizmoColor = Color.red * 0.5f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(agent.args.previousPoint, Vector3.one * 0.5f);
            }
            if (agent.args.path != null && !agent.args.path.completed)
            {
                gizmoColor = Color.blue;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(agent.args.path.end, Vector3.one * 0.75f);
            }
        }

        if(agentController.playerAgent.args.playerPath != null && agentController.playerAgent.args.playerPath.Count > 0)
        {
            foreach (AgentPath path in agentController.playerAgent.args.playerPath)
            {
                gizmoColor = Color.magenta;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(path.start, Vector3.one * 0.35f);
                Gizmos.DrawWireCube(path.end, Vector3.one * 0.35f);
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
        if(agent.args.type == AgentType.Enemy)
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
            Debug.Log($"Successfully released occupation of {agent.args.type}");
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
            Debug.Log($"Successfully released occupation of {releasingAgent.args.type}");
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