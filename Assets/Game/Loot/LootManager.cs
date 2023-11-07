using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public GameObject lootRockPrefab;
    //spawning ore loot
    //spawning stone loot
    public CellLoot InstantiateLoot_Ore(Vector3 spawnPoint, Ore ore)
    {
        GameObject lootObject = Instantiate(lootRockPrefab, spawnPoint, Quaternion.identity, transform);
        CellLoot loot = new CellLoot();
        loot.lootName = ore.name;
        loot.amount = 1;
        loot.instantiatedObject = lootObject;
        lootObject.GetComponentInChildren<SpriteRenderer>().color = ore.color;
        lootObject.GetComponentInChildren<SpriteAnimator>().PlayAnimation("Rock_Large");
        return loot;
    }
}
public class CellLoot
{
    public string lootName;
    public int amount;
    public GameObject instantiatedObject;
}