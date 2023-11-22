using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DefenderIO
{
    public static List<Defender> LoadDefendersFromJSON(DefenderBase[] defenderBases)
    {
        if (defenderBases == null || defenderBases.Length == 0)
            return null;
        string persistentDataPath = Application.persistentDataPath;
        List<Defender> defendersLoaded = new List<Defender>();
        foreach (DefenderBase item in defenderBases)
        {
            string directory = Path.Combine(persistentDataPath, item.name);
            if (!Directory.Exists(directory))
                continue;
            string[] fileEntries = Directory.GetFiles(directory);
            foreach (string fileEntry in fileEntries) {
                string defenderJSON_string = File.ReadAllText(fileEntry);
                DefenderJSON defenderJSON = new DefenderJSON(defenderJSON_string);
                Defender defender = new Defender(defenderJSON, item);
                defendersLoaded.Add(defender);
            }
        }

        return defendersLoaded;
    }
    public static void SaveDefendersToJSON(List<Defender> defenderList)
    {
        if (defenderList == null || defenderList.Count == 0)
            return;
        string persistentDataPath = Application.persistentDataPath;
        foreach (var item in defenderList)
        {
            string directory = Path.Combine(persistentDataPath, item.defenderName);
            string path = Path.Combine(persistentDataPath, item.defenderName, item.defenderKey.ToString());
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            DefenderJSON defenderJSON = new DefenderJSON(item);
            string defenderJSON_string = JsonUtility.ToJson(defenderJSON);
            File.WriteAllText(path, defenderJSON_string);
            Debug.Log($"Saving {item.defenderName}_{item.defenderKey} to {path}");
        }
    }
}
[Serializable]
public class DefenderJSON
{
    public string defenderBaseName;
    public int defenderKey;
    public int maxHealth, health;
    public int maxExp, exp;
    public DefenderJSON(Defender defender)
    {
        defenderBaseName = defender.defenderName;
        defenderKey = defender.defenderKey;
        maxHealth = defender.maxHealth;
        health = defender.health;
        maxExp = defender.maxExp;
        exp = defender.exp;
    }
    public DefenderJSON(string defenderJSON_string)
    {
        JsonUtility.FromJsonOverwrite(defenderJSON_string, this);
    }
}