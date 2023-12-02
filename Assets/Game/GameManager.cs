using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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

    public AgentController agentController;
    PlayerControls input;
    //Defender character
    //Enemy character
    public Player player;
    public float pitSpawnTime;
    float pitSpawnTimer;

    #region Graphics Raycast
    public GraphicRaycaster raycaster;
    PointerEventData pointerEventData;
    public EventSystem eventSystem;
    #endregion
    #region Defenders List
    public List<DefenderData> defendersLoaded;

    #endregion
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

            #region Check if tap was on UI elements
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = screenPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);
            if(results.Count > 0)
            {
                return;
            }
            #endregion

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
        #region AgentController Start
        allAgents = new List<IAgent>();
        agentController = new AgentController();
        agentController.player = characterGenerator.CreatePlayer(Vector3Int.zero);
        ((IAgent)agentController.player).args.onPlayerNoMovesLeft += agentController.Agents_RefreshMovesLeft;

        onTap += agentController.Player_PathCalculate;
        #region Construct Enemies
        agentController.enemies = new List<Enemy>()
        {
            characterGenerator.CreateEnemy(new Vector3Int(2,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(0,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-2,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(3,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(1,3,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-1,2,0)),
            characterGenerator.CreateEnemy(new Vector3Int(-3,3,0)),
        };
        #endregion
        #endregion

        characterGenerator.SaveTestDefenders();
        defendersLoaded = DefenderIO.LoadDefendersFromJSON(characterGenerator.defenderBases);
        #region Construct Defenders
        agentController.defenders = new List<Defender>()
        {
            characterGenerator.CreateDefender(defendersLoaded[0]),
            characterGenerator.CreateDefender(defendersLoaded[1]),
            characterGenerator.CreateDefender(defendersLoaded[2]),
            characterGenerator.CreateDefender(defendersLoaded[3]),
            characterGenerator.CreateDefender(defendersLoaded[4]),
            characterGenerator.CreateDefender(defendersLoaded[5]),
        };
        agentController.activeDefenders = new List<Defender>();
        #endregion

        foreach (IAgent agent in agentController.enemies)
        {
            agent.args.target = agentController.player;
        }
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

    public delegate PlaceDefenderRequestArgs PlaceDefender();
    public static event PlaceDefender PlaceDefenderRequest; 
    public class PlaceDefenderRequestArgs
    {
        public Defender defenderToPlace;
        public Vector3Int position;
        public PlaceDefenderRequestArgs(Defender defenderToPlace, Vector3Int position)
        {
            this.defenderToPlace = defenderToPlace;
            this.position = position;
        }
    }

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
    }
    private void FixedUpdate()
    {
        ((IAgent)agentController.player).args.MoveAlongPath(Time.fixedDeltaTime);
        bool defenderPlaced = false;
        if (PlaceDefenderRequest != null)
        {
            foreach (Delegate item in PlaceDefenderRequest.GetInvocationList())
            {
                PlaceDefenderRequestArgs args = item.DynamicInvoke() as PlaceDefenderRequestArgs;
                Defender defenderToPlace = args.defenderToPlace;
                Vector3Int pos = args.position;
                Debug.Log($"Placing defender {defenderToPlace.defenderData.defenderName}_{defenderToPlace.defenderData.defenderKey}");
                defenderToPlace.gameObject.SetActive(true);
                defenderToPlace.transform.position = CellToWorld(pos);
                if (agentController.defenders.Contains(defenderToPlace))
                {
                    agentController.activeDefenders.Add(defenderToPlace);
                    agentController.defenders.Remove(defenderToPlace);
                }
            }
            PlaceDefenderRequest = null;
            defenderPlaced = true;
        }

        if (agentController.enemies != null && agentController.enemies.Count > 0)
        {
            if(defenderPlaced)
            {
                //Debug.Log("enemy retargeting should be done");
                //Go through all the active defenders
                for (int i = 0; i < agentController.activeDefenders.Count; i++)
                {
                    IAgent defender = agentController.activeDefenders[i];
                    if (defender.args.targetedBy.Count >= 4)
                        continue; //Ignore the ones that are targeted by more than 4
                    //Ignore the ones that are targeted by more than there are adjacent spaces
                    foreach (IAgent enemy in agentController.enemies)
                    {
                        int freeSpaces = agentController.AvailableAdjacentSpaces(enemy, agentController.activeDefenders[i]);
                        if (defender.args.targetedBy.Count >= freeSpaces)
                            break;
                        if (enemy.args.target != null)
                        {
                            //Don't try to swap target if we have already targeted a defender
                            if(agentController.activeDefenders.Contains(enemy.args.target as Defender))
                                continue;
                            enemy.args.target.args.targetedBy.Remove(enemy);
                        }
                        enemy.args.target = agentController.activeDefenders[i];
                        defender.args.targetedBy.Add(enemy);
                    }
                }
            }

            foreach (Enemy enemy in agentController.enemies)
            {
                //Debug.Log($"agent.args.hasInstruction = {agent.args.hasInstruction}");
                //Debug.Log($"agent.args.movesLeft = {agent.args.movesLeft}");
                //Debug.Log($"agent.args.path == null = {agent.args.path == null}");
                //((ITargeting)enemy).UpdateTarget(potentialTargets);
                if (((IAgent)enemy).args.hasInstruction)
                {
                    ((IAgent)enemy).args.MoveAlongPath(Time.fixedDeltaTime);
                }
            }
            agentController.Enemies_PathCalculate();
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
    public Vector3 WorldToScreenPosition(Vector3 worldPosition)
    {
        return mainCamera.WorldToScreenPoint(worldPosition);
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
            if (agent.args.target != null)
            {
                gizmoColor = Color.red;
                gizmoColor.a = 1f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(agent.args.worldPos, agent.args.target.args.worldPos);
            }
            if (!agent.args.hasInstruction)
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

        if(((IAgent)agentController.player).args.playerPath != null && ((IAgent)agentController.player).args.playerPath.Count > 0)
        {
            foreach (AgentPath path in ((IAgent)agentController.player).args.playerPath)
            {
                gizmoColor = Color.magenta;
                gizmoColor.a = 0.5f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(path.start, Vector3.one * 0.35f);
                Gizmos.DrawWireCube(path.end, Vector3.one * 0.35f);
            }
        }
    }
    #region Old Code
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
    #endregion
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
    public float DistanceFromPlayer(IAgent agent)
    {
        return Vector3.Distance(agent.args.cellPos, ((IAgent)agentController.player).args.cellPos);
    }
    public Vector3 GetPlayerWorldPosition()
    {
        return ((IAgent)agentController.player).args.worldPos;
    }
    public void TryLootAtCell(Vector3Int cellPos, IAgent agent)
    {
        CellData cellData = GetCellDataAtPosition(cellPos);
        cellData.TryLoot(agent);
    }
    public List<Defender> GetDefenders()
    {
        return agentController.defenders;
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
        bool occupationSucceeded = false;
        if(occupyingAgent == null)
        {
            occupyingAgent = agent;
            GameManager.ins.Update_OccupiedTiles(this, agent);
            occupationSucceeded = true;
        }
        else
        {
            if (reservingAgent == agent)
                occupationSucceeded = true;
        }
        return occupationSucceeded;
    }
    public void TryLoot(IAgent agent)
    {
        if(!agent.args.allowedToLoot.HasFlag(LootType.All))
        {
            if (agent.args.allowedToLoot.HasFlag(LootType.None))
                return;
            if (!agent.args.allowedToLoot.HasFlag(loot.type))
                return;
        }
        if (loot != null)
        {
            //Debug.Log($"Agent {agent.args.type} trying to loot {loot.lootName}");
            agent.args.PickupLoot(loot);
            GameObject.Destroy(loot.instantiatedObject);
            loot = null;
        }
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
public class AgentController
{
    public Player player;
    public List<Enemy> enemies;
    public List<Defender> defenders;
    public List<Defender> activeDefenders;
    public void Player_PathCalculate(CellData cellData)
    {
        ((IAgent)player).args.ResetCompletedFullPathEvent();
        bool targetingStone = cellData.durability > 0;
        if (targetingStone)
        {
            #region Set up pickaxe animation stuff
            player.tool.toolTargetCellPos = cellData.cellPosition;
            player.tool.toolTargetCellCenterWorldPos = cellData.cellCenterWorldPosition;
            #endregion
            List<Vector3Int> shortestPathToNeighbour =
                cellData.GetPathToClosestCardinalNeighbour(player);
            #region Case : Where we are right next to the stone we targeted
            if (Vector3Int.Distance(((IAgent)player).args.cellPos, cellData.cellPosition) == 1)
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
                    ((IAgent)player).args.moveInterpolationType);
                fullPath.Add(path);
            }
            ((IAgent)player).args.playerPath = fullPath;
            ((IAgent)player).args.playerPathIndex = 0;
            #endregion
            ((IAgent)player).args.onPlayerCompletedFullPath += player.StartMiningAnimation;
            #endregion
        }
        else
        {
            List<Vector3Int> points = Pathfinding.aStarNew(
                ((IAgent)player),
                ((IAgent)player).args.cellPos,
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
                    ((IAgent)player).args.moveInterpolationType);
                fullPath.Add(path);
            }
            ((IAgent)player).args.playerPath = fullPath;
            ((IAgent)player).args.playerPathIndex = 0;
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
            if (agent.args.path != null)
            {
                agentNextStepList.Add(agent.args.path.end);
            }
        }
        foreach(IAgent agent in activeDefenders)
        {
            if (!agent.args.transform.gameObject.activeSelf)
                continue;
            agentCurrentPositionList.Add(agent.args.cellPos);
            if(agent.args.path != null)
            {
                agentNextStepList.Add(agent.args.path.end);
            }
        }
        #endregion

        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            IAgent agent = enemy;
            //path to write to
            if (agent.args.hasInstruction)
                continue;
            if (agent.args.movesLeft <= 0)
                continue;
            if (agent.args.target == null)
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
                    if (neighbourPosition == item && item != agent.args.cellPos)
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
                    agent.args.target.args.cellPos,
                    agent.GetInaccessibleTilemaps(),
                    invalidNeighbourPositions
                    );
            #endregion
            #region Setup Non Trivial End For Path
            if (points != null && points.Count > 1)
            {
                if (points[points.Count - 2] != agent.args.target.args.cellPos)
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
    public int AvailableAdjacentSpaces(IAgent agent, IAgent targetAgent)
    {
        Tilemap[] inaccessibleTilemaps = agent.GetInaccessibleTilemaps();
        #region Current + Next step list
        List<Vector3Int> agentCurrentPositionList = new List<Vector3Int>();
        List<Vector3Int> agentNextStepList = new List<Vector3Int>();
        foreach (IAgent enemy in enemies)
        {
            if (enemy == agent)
                continue;
            agentCurrentPositionList.Add(enemy.args.cellPos);
            if (enemy.args.path != null)
            {
                agentNextStepList.Add(enemy.args.path.end);
            }
        }
        foreach (IAgent activeDefender in activeDefenders)
        {
            if (!activeDefender.args.transform.gameObject.activeSelf)
                continue;
            agentCurrentPositionList.Add(activeDefender.args.cellPos);
            if (activeDefender.args.path != null)
            {
                agentNextStepList.Add(activeDefender.args.path.end);
            }
        }
        #endregion
        List<Vector3Int> neighbourPositions = GameManager.ins.GetCardinalNeighbourPositions(targetAgent.args.cellPos, false);
        List<Vector3Int> invalidNeighbourPositions = new List<Vector3Int>();
        foreach (Vector3Int neighbourPosition in neighbourPositions)
        {
            bool neighbourPositionInvalid = false;
            if(agentCurrentPositionList.Contains(neighbourPosition))
            {
                neighbourPositionInvalid = true;
            }
            if(agentNextStepList.Contains(neighbourPosition))
            {
                neighbourPositionInvalid = true;
            }
            foreach (Tilemap item in inaccessibleTilemaps)
            {
                if (item.GetTile(neighbourPosition) != null)
                {
                    neighbourPositionInvalid = true;
                }
            }
            if (neighbourPositionInvalid)
                invalidNeighbourPositions.Add(neighbourPosition);
        }
        Debug.Log($"{agent.args.transform.name} check -> {targetAgent.args.transform.name} has {4 - invalidNeighbourPositions.Count} free spaces.");
        return 4 - invalidNeighbourPositions.Count;
    }
}