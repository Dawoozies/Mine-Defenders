using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyBasePrefab;
    public EnemyBase[] enemyBases;
    #region Defender
    public GameObject defenderPrefab;
    public DefenderBase[] defenderBases;
    #endregion
    public Player CreatePlayer(Vector3Int cellToSpawnAt)
    {
        Player player = Instantiate(playerPrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Player>();
        IAgent playerAgent = player;
        playerAgent.health = 100;
        playerAgent.args.type = AgentType.Player;
        return player;
    }
    public Enemy CreateEnemy(Vector3Int cellToSpawnAt)
    {
        Enemy createdEnemy = Instantiate(enemyBasePrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Enemy>();
        createdEnemy.Initialise(enemyBases[Random.Range(0, enemyBases.Length)]);
        ((IAgent)createdEnemy).args.isActive = true;
        ((IAgent)createdEnemy).args.type = AgentType.Enemy;
        return createdEnemy;
    }

    public List<DefenderData> testDefenders;
    public void SaveTestDefenders()
    {
        if (testDefenders == null || testDefenders.Count == 0)
            return;
        DefenderIO.SaveDefendersToJSON(testDefenders);
    }
    public Defender CreateDefender(DefenderData defenderData, Vector3Int cellToSpawnAt)
    {
        Defender createdDefender = Instantiate(defenderPrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Defender>();
        createdDefender.Initialise(defenderData);
        ((IAgent)createdDefender).args.isActive = true;
        ((IAgent)createdDefender).args.type = AgentType.Defender;
        return createdDefender;
    }
}