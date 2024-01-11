using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
public class LootManager : MonoBehaviour
{
    public GameObject lootRockPrefab;
    //spawning ore loot
    //spawning stone loot
    public Dictionary<string, int> depositedLoot = new Dictionary<string, int>();
    public List<string> lootStrings = new List<string>();
    public CellLoot InstantiateLoot_Ore(Vector3 spawnPoint, Ore ore)
    {
        GameObject lootObject = Instantiate(lootRockPrefab, spawnPoint, Quaternion.identity, transform);
        CellLoot loot = new CellLoot();
        loot.lootName = ore.name;
        loot.amount = Random.Range(0, 4) + 1;
        loot.type = LootType.Ore;
        loot.instantiatedObject = lootObject;
        //lootObject.GetComponentInChildren<SpriteRenderer>().color = ore.color;
        lootObject.GetComponentInChildren<SpriteAnimator>().PlayAnimation(loot.amount - 1);
        return loot;
    }
    public void DepositLoot(Dictionary<string, int> lootToDeposit)
    {
        foreach (string depositKey in lootToDeposit.Keys)
        {
            bool hasKey = depositedLoot.ContainsKey(depositKey);
            if (hasKey) depositedLoot[depositKey] = depositedLoot[depositKey] + lootToDeposit[depositKey];
            else depositedLoot.Add(depositKey, lootToDeposit[depositKey]);
        }
        lootStrings.Clear();
        foreach (string lootName in depositedLoot.Keys)
        {
            string lootString = $"<lootName = {lootName}, amount = {depositedLoot[lootName]}>";
            Debug.Log(lootString);
            lootStrings.Add(lootString);
        }
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