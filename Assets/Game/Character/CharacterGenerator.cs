using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject enemyBasePrefab;
    public EnemyBase[] enemyBases;
    public List<CharacterAgent> enemyAgents = new List<CharacterAgent>();
    public Player CreatePlayer(Vector3Int cellToSpawnAt)
    {
        return Instantiate(playerPrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Player>();
    }
    public Enemy CreateEnemy(Vector3Int cellToSpawnAt)
    {
        Enemy createdEnemy = Instantiate(enemyBasePrefab, GameManager.ins.CellToWorld(cellToSpawnAt), Quaternion.identity, transform).GetComponent<Enemy>();
        createdEnemy.Initialise(enemyBases[0]);
        return createdEnemy;
    }
    public Transform ManagedStart()
    {
        return Instantiate(playerPrefab, transform).transform;
    }
    public Transform CreateEnemy(Vector3 spawnPosition)
    {
        Transform enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform).transform;
        enemyAgents.Add(enemy.GetComponent<CharacterAgent>());
        return enemy;
    }
}
