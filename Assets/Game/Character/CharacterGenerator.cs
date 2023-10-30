using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public void ManagedStart()
    {
        Instantiate(playerPrefab);
    }
}
