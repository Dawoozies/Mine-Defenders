using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
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
        loot.amount = Random.Range(0, 4) + 1;
        loot.type = LootType.Ore;
        loot.instantiatedObject = lootObject;
        lootObject.GetComponentInChildren<SpriteRenderer>().color = ore.color;
        lootObject.GetComponentInChildren<SpriteAnimator>().PlayAnimation(loot.amount - 1);
        return loot;
    }
}
public class CellLoot
{
    public string lootName;
    public int amount;
    public LootType type;
    public GameObject instantiatedObject;
}
[Flags]
public enum LootType
{ 
    None = 0,
    All = 1,
    Ore = 2,
}