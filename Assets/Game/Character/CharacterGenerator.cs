using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyBasePrefab;
    public EnemyBase[] enemyBases;
    public Player CreatePlayer(Vector3Int cellToSpawnAt)
    {
        Player player = Instantiate(playerPrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Player>();
        IAgent playerAgent = player;
        playerAgent.args.health = 100;
        return player;
    }
    public Enemy CreateEnemy(Vector3Int cellToSpawnAt)
    {
        Enemy createdEnemy = Instantiate(enemyBasePrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Enemy>();
        createdEnemy.Initialise(enemyBases[Random.Range(0, enemyBases.Length)]);
        return createdEnemy;
    }
}
