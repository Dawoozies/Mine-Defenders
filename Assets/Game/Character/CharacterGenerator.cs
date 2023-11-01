using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public List<CharacterAgent> enemyAgents = new List<CharacterAgent>();
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
