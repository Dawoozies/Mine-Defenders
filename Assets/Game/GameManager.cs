using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;
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

    //public Hashtable cellTable;
    //public Hashtable uncoveredPitCenters;
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

            onTap?.Invoke(level[cellPosition]);
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
        levelGenerator.GenerateLevel();

        //player = characterGenerator.ManagedStart();
        //playerLastCellPos = WorldToCell(player.position);
        //playerAgent = player.GetComponent<CharacterAgent>();
        //characterGenerator.CreateEnemy();
        #region AgentController Start
        agentController = new AgentController();
        agentController.player = characterGenerator.CreatePlayer(Vector3Int.zero);
        //((IAgent)agentController.player).args.onPlayerNoMovesLeft += agentController.Agents_RefreshMovesLeft;

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
        agentController.defenders = new List<Defender>();
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
        public DefenderData defenderDataToPlace;
        public Vector3Int position;
        public PlaceDefenderRequestArgs(DefenderData defenderDataToPlace, Vector3Int position)
        {
            this.defenderDataToPlace = defenderDataToPlace;
            this.position = position;
        }
    }

    public delegate void OnPlayerLootPickup(string lootName, int addedAmount, int totalAmount);
    public static event OnPlayerLootPickup onPlayerLootPickup;
    public delegate void OnCellBreak(CellData cellBrokenData);
    public static event OnCellBreak onCellBreak;
    public Dictionary<Vector3Int, CellData> level => levelGenerator.level;
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
        if (OnPlayerPositionBufferUpdated != null)
        {
            Vector3Int playerCellPosition = OnPlayerPositionBufferUpdated.Invoke();
            Vector3 playerWorldPosition = CellToWorld(playerCellPosition);
            CameraTrackAnimation(playerWorldPosition);
            OnPlayerPositionBufferUpdated = null;
        }
        if (OnPlayerOccupiedNewCell != null)
        {
            CellData cellData = OnPlayerOccupiedNewCell.Invoke();
            PlayerOccupiedNewCellUpdatedEvent?.Invoke(cellData);

            OnPlayerOccupiedNewCell = null;
        }
        //Do pit spawning enemies
        if (uncoveredPitCenters != null && uncoveredPitCenters.Keys.Count > 0)
        {
            pitSpawnTimer += Time.deltaTime;
            if (pitSpawnTimer > pitSpawnTime)
            {
                foreach (Vector3Int item in uncoveredPitCenters.Keys)
                {
                    //Debug.Log($"Spawn enemies at {item}");
                    if (agentController.CheckAgentCanSpawnAtPosition(item))
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
    public float moveUpdateTime;
    float moveUpdateTimer;
    private void FixedUpdate()
    {
        if (PlaceDefenderRequest != null)
        {
            foreach (Delegate item in PlaceDefenderRequest.GetInvocationList())
            {
                PlaceDefenderRequestArgs args = item.DynamicInvoke() as PlaceDefenderRequestArgs;
                //first we need to look in current defenders to see if already placed
                bool defenderAlreadyExists = agentController.DefenderAlreadyExists(args.defenderDataToPlace);
                if (defenderAlreadyExists)
                    continue;
                agentController.defenders.Add(characterGenerator.CreateDefender(args.defenderDataToPlace, args.position));
            }
            PlaceDefenderRequest = null;
            if (agentController.defenders != null)
            {
                foreach (IAgent defender in agentController.defenders)
                {
                    defender.Retarget();
                }
            }
            if (agentController.enemies != null)
            {
                foreach (IAgent enemy in agentController.enemies)
                {
                    enemy.Retarget();
                }
            }
        }
        ((IAgent)agentController.player).args.MoveAlongPath(Time.fixedDeltaTime);
        #region Agent movement
        if(moveUpdateTimer > 0)
        {
            moveUpdateTimer -= Time.fixedDeltaTime;
        }
        if (moveUpdateTimer <= 0)
        {
            agentController.Agents_RefreshMovesLeft();
            moveUpdateTimer = moveUpdateTime;
        }
        if (agentController.defenders != null)
        {
            foreach (IAgent defender in agentController.defenders)
            {
                if (defender.args.hasInstruction)
                {
                    defender.args.MoveAlongPath(Time.fixedDeltaTime);
                }
                if(defender.args.timeSpentNotMoving >= 30)
                {
                    Debug.LogError("Defender has been still for 30 seconds");
                }
            }
        }
        if (agentController.enemies != null)
        {
            foreach (IAgent enemy in agentController.enemies)
            {
                if (enemy.args.hasInstruction)
                {
                    enemy.args.MoveAlongPath(Time.fixedDeltaTime);
                }
                if (enemy.args.timeSpentNotMoving >= 30)
                {
                    Debug.LogError("Defender has been still for 30 seconds");
                }
            }
        }
        #endregion
        agentController.NonPlayerAgents_PathCalculate();
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

        foreach (IAgent agent in agentController.defenders)
        {
            if (agent.args.target != null)
            {
                gizmoColor = Color.blue;
                gizmoColor.a = 1f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(agent.args.worldPos, agent.args.target.args.worldPos);
            }
        }
    }

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

        onCellBreak?.Invoke(cellData);
    }
    public void TryLootAtCell(Vector3Int cellPos, IAgent agent)
    {
        CellData cellData = GetCellDataAtPosition(cellPos);
        CellLoot loot = cellData.loot;
        if (loot == null)
            return;
        if (!agent.args.allowedToLoot.HasFlag(LootType.All))
        {
            if (agent.args.allowedToLoot.HasFlag(LootType.None))
                return;
            if (!agent.args.allowedToLoot.HasFlag(loot.type))
                return;
        }
        if (loot != null)
        {
            agent.args.PickupLoot(loot);
            GameObject.Destroy(loot.instantiatedObject);
            onPlayerLootPickup?.Invoke(loot.lootName, loot.amount, agent.args.TotalLootCount());
            cellData.loot = null;
        }
    }
    public List<Defender> GetDefenders()
    {
        return agentController.defenders;
    }
    public List<Enemy> GetEnemies()
    {
        return agentController.enemies;
    }
    public Vector2 GetScreenPosition(Vector3 position)
    {
        return mainCamera.WorldToScreenPoint(position);
    }
    public void DepositLoot(Dictionary<string, int> lootToDeposit)
    {
        lootManager.DepositLoot(lootToDeposit);
    }
    public List<string> PlayerLootStringList()
    {
        Dictionary<string, int> playerLoot = (agentController.player as IAgent).args.lootDictionary;
        if (playerLoot == null || playerLoot.Count == 0)
            return null;
        List<string> lootStringList = new List<string>();
        foreach (string lootName in playerLoot.Keys)
        {
            string lootString = $"{playerLoot[lootName]}x {lootName.ToUpper()}";
            lootStringList.Add(lootString);
        }
        return lootStringList;
    }
    public Vector3 ScreenToWorld(Vector2 screenPos)
    {
        return mainCamera.ScreenToWorldPoint(screenPos);
    }
    public void DragMovePlayer(Vector2 screenPos)
    {
        Vector3 worldPos = ScreenToWorld(screenPos);
        //Vector3Int cellPosition = WorldToCell(worldPos);
        //agentController.Player_PathCalculate((CellData)cellTable[cellPosition]);
    }
}
//public class CellData
//{
//    public Vector3Int cellPosition;
//    public Vector3 cellCenterWorldPosition;
//    public bool isPlayerSpawnArea;
//    public bool isLevelBoundary;
//    public Ore ore;
//    public int durability;
//    public bool isPit;
//    public bool isUncoveredPit;
//    public bool isPitCenter;
//    public CellLoot loot;
//    public List<CellData> neighbours;
//    public bool isTraversable()
//    {
//        if (durability > 0)
//            return false;
//        if (isPit || isUncoveredPit || isPitCenter)
//            return false;
//        if (isLevelBoundary)
//            return false;
//        return true;
//    }
//    public int traversableNeighbours()
//    {
//        if (neighbours == null)
//            neighbours = GetCardinalNeighbours(false);
//        if (neighbours == null)
//            return 0;
//        int traversableNeighbours = 0;
//        foreach (CellData item in neighbours)
//        {
//            if (item == null)
//                continue;
//            if (item.isTraversable())
//                traversableNeighbours++;
//        }
//        return traversableNeighbours;
//    }
//    public List<CellData> GetAllNeighbours(bool includeSelf)
//    {
//        return GameManager.ins.GetAllNeighboursAroundCell(cellPosition, includeSelf);
//    }
//    public List<CellData> GetCardinalNeighbours(bool includeSelf)
//    {
//        if (neighbours != null)
//            return neighbours;
//        neighbours = GameManager.ins.GetCardinalNeighboursAroundCell(cellPosition, false);
//        return neighbours;
//    }
//    public List<Vector3Int> GetPathToClosestCardinalNeighbour(IAgent agent)
//    {
//        List<CellData> neighbours = GetCardinalNeighbours(true);
//        List<Vector3Int> shortestPath = null;
//        foreach (CellData neighbour in neighbours)
//        {
//            List<Vector3Int> path;
//            path = Pathfinding.aStarWithIgnore(agent.args.cellPos, neighbour.cellPosition, agent.GetInaccessibleTilemaps(), null);
//            if (path == null || path.Count == 0)
//                continue;
//            if (shortestPath == null)
//                shortestPath = path;
//            else
//                shortestPath = path.Count < shortestPath.Count ? path : shortestPath;
//        }
//        return shortestPath;
//    }
//    public List<Vector3Int> GetPathToCell(IAgent agent)
//    {
//        if(durability > 0)
//        {
//            return GetPathToClosestCardinalNeighbour(agent);
//        }
//        List<Vector3Int> path = Pathfinding.aStarWithIgnore(agent.args.cellPos, cellPosition, agent.GetInaccessibleTilemaps(), null);
//        return path;
//    }
//    public void CellInfoDebug()
//    {
//        Debug.Log("[--------------------------------]");
//        Debug.Log($"cellPosition = {cellPosition}");
//        Debug.Log($"cellCenterWorldPosition = {cellCenterWorldPosition}");
//        Debug.Log($"isPlayerSpawnArea = {cellPosition}");
//        Debug.Log($"isLevelBoundary = {cellCenterWorldPosition}");
//        string oreDebug = ore == null ? "ore = null" : $"ore = {ore.name}";
//        Debug.Log(oreDebug);
//        Debug.Log($"durability = {durability}");
//        Debug.Log($"isPit = {isPit}");
//        Debug.Log($"isUncoveredPit = {isUncoveredPit}");
//        Debug.Log("[--------------------------------]");
//    }
//}
public class AgentController
{
    public Player player;
    public List<Enemy> enemies;
    public List<Defender> defenders;
    List<Vector3Int> currentPositionsList = new List<Vector3Int>();
    List<Vector3Int> nextStepList = new List<Vector3Int>();
    public void Player_PathCalculate(CellData cellData)
    {
        IAgent playerAgent = player as IAgent;
        playerAgent.args.ResetCompletedFullPathEvent();
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
                player,
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
    public void NonPlayerAgents_PathCalculate()
    {
        #region Current positions + next step list
        currentPositionsList = new List<Vector3Int>();
        nextStepList = new List<Vector3Int>();
        if(enemies != null)
        {
            foreach(IAgent agent in enemies)
            {
                if (agent.args.isDead)
                    continue;
                if (!agent.args.isActive)
                    continue;
                currentPositionsList.Add(agent.args.cellPos);
                if (agent.args.path != null)
                    nextStepList.Add(agent.args.path.end);
            }
        }
        if(defenders != null)
        {
            foreach (IAgent agent in defenders)
            {
                if (agent.args.isDead)
                    continue;
                if (!agent.args.isActive)
                    continue;
                currentPositionsList.Add(agent.args.cellPos );
                if (agent.args.path != null)
                    nextStepList.Add(agent.args.path.end);
            }
        }
        #endregion
        int agentIndex = 0;
        if(defenders != null)
        {
            foreach (IAgent agent in defenders)
            {
                if (agent.args.isDead)
                    continue;
                if (!agent.args.isActive)
                    continue;
                bool result = NonPlayerAgent_TryGeneratePath(agent, agentIndex);
                //Debug.Log($"defender. Index = {agentIndex} Result = {result}");
                if (!result)
                    continue;
                nextStepList.Add(agent.args.path.end);
                agentIndex++;
            }
        }
        if(enemies != null)
        {
            foreach (IAgent agent in enemies)
            {
                if (agent.args.isDead)
                    continue;
                if (!agent.args.isActive)
                    continue;
                bool result = NonPlayerAgent_TryGeneratePath(agent, agentIndex);
                //Debug.Log($"enemy. Index = {agentIndex} Result = {result}");
                if (!result)
                    continue;
                nextStepList.Add(agent.args.path.end);
                agentIndex++;
            }
        }
    }
    bool NonPlayerAgent_TryGeneratePath(IAgent agent, int agentIndex)
    {
        if (agent.args.hasInstruction)
            return false;
        if (agent.args.movesLeft <= 0)
            return false;
        if (agent.args.target == null)
            return false;
        AgentPath path = new AgentPath(agent.args.cellPos, agent.args.cellPos, agent.args.moveInterpolationType);
        agent.args.path = path;

        List<Vector3Int> invalidPositions = InvalidNeighbourPositions(agent, agentIndex);
        if (invalidPositions.Count == 4)
        {
            return false;
        }

        List<Vector3Int> points = new List<Vector3Int>();
        points = Pathfinding.aStarWithIgnore(
                agent.args.cellPos,
                agent.args.target.args.cellPos,
                agent.GetInaccessibleTilemaps(),
                invalidPositions
            );

        if(points != null && points.Count > 1)
        {
            if (points[points.Count - 2] != agent.args.target.args.cellPos)
            {
                path.end = points[points.Count - 2];
                if(!agent.args.hasInstruction)
                    agent.args.hasInstruction = true;
                return true;
            }
        }
        return false;
    }
    List<Vector3Int> InvalidNeighbourPositions(IAgent agent, int agentIndex)
    {
        List<Vector3Int> neighbourPositions = GameManager.ins.GetCardinalNeighbourPositions(agent.args.cellPos, false);
        List<Vector3Int> invalidPositions = new List<Vector3Int>();
        foreach (Vector3Int neighbourPos in neighbourPositions)
        {
            if(currentPositionsList.Contains(neighbourPos))
            {
                invalidPositions.Add(neighbourPos);
                continue;
            }
            if(agentIndex != 0 && nextStepList.Contains(neighbourPos))
            {
                invalidPositions.Add(neighbourPos);
                continue;
            }
            if(agent.args.previousPoint.z != -1 && neighbourPos == agent.args.previousPoint)
            {
                invalidPositions.Add(neighbourPos);
                continue;
            }
        }
        return invalidPositions;
    }
    public void Agents_RefreshMovesLeft()
    {
        foreach (IAgent agent in defenders)
        {
            agent.args.RefreshMovesLeft();
            agent.Retarget();
        }
        foreach (IAgent agent in enemies)
        {
            agent.args.RefreshMovesLeft();
            agent.Retarget();
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
    public List<IAgent> GetAgentsAdjacentToPosition(Vector3Int cellPos, List<AgentType> agentTypes, bool includeSelf)
    {
        List<IAgent> foundAgents = new List<IAgent>();
        List<Vector3Int> neighbourPositions = GameManager.ins.GetCardinalNeighbourPositions(cellPos, includeSelf);
        if(agentTypes.Contains(AgentType.Player))
        {
            if (neighbourPositions.Contains(player.cellPos))
                foundAgents.Add(player);
        }
        if(agentTypes.Contains(AgentType.Enemy))
        {
            foreach (IAgent enemy in enemies)
            {
                if (neighbourPositions.Contains(enemy.args.cellPos))
                    foundAgents.Add(enemy);
            }
        }
        if(agentTypes.Contains(AgentType.Defender))
        {
            foreach (IAgent defender in defenders)
            {
                if (neighbourPositions.Contains(defender.args.cellPos))
                    foundAgents.Add(defender);
            }
        }
        return foundAgents;
    }
    public IAgent AdjacencyTarget(IAgent agent, IAgent currentTarget, List<AgentType> agentTypes)
    {
        List<IAgent> adjacentAgents = GetAgentsAdjacentToPosition(agent.args.cellPos, agentTypes, false);
        if (adjacentAgents.Count == 0)
            return null;

        if (adjacentAgents.Contains(currentTarget))
            return currentTarget;

        int randomIndex = Random.Range(0, adjacentAgents.Count);
        return adjacentAgents[randomIndex];
    }
    public bool DefenderAlreadyExists(DefenderData defenderDataToPlace)
    {
        if (defenders == null || defenders.Count == 0)
            return false;
        foreach (Defender defender in defenders)
        {
            if (defender.defenderData.defenderKey == defenderDataToPlace.defenderKey)
                return true;
        }
        return false;
    }
}