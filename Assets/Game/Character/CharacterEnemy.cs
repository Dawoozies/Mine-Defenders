using UnityEngine;
public class CharacterEnemy : MonoBehaviour
{
    CharacterAgent agent;
    void Start()
    {

        agent = GetComponent<CharacterAgent>();
        agent.isEnemy = true;
        //GameManager.PlayerPositionUpdatedEvent += MoveToPlayer;
        agent.MovementCompleteEvent += Attack;
        agent.EnemyMovementOrder("MoveToPlayer", GameManager.ins.playerLastCellPos);

    }
    void MoveToPlayer(Vector3Int playerCellPos, Vector3 playerWorldPos)
    {
        agent.EnemyMovementOrder("MoveToPlayer", playerCellPos);
    }
    void Attack(string orderName)
    {
        //Animation like a pickaxe swing
        Debug.Log("Got next to player");
    }
}
