using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Tilemaps;

public class PitManager : MonoBehaviour
{
    public class UncoveredPit
    {
        (Vector3Int, Vector3) pitPositions;
        float nextSpawn;
        public delegate bool OnTimeReached();
        public OnTimeReached onTimeReached;
        public UncoveredPit((Vector3Int, Vector3) pitPositions)
        {
            this.pitPositions = pitPositions;
            nextSpawn = Time.time + 0.5f;
        }

        public void Update() {
            if (Time.time > nextSpawn) {
                onTimeReached?.Invoke();
                nextSpawn = Time.time + 0.5f;
            }
        }
    }
    List<UncoveredPit> uncoveredPits;
    public void ManagedStart()
    {
        uncoveredPits = new List<UncoveredPit>();
        GameManager.PitUncoveredEvent += OnPitUncovered;
    }
    void OnPitUncovered((Vector3Int, Vector3) pit)
    {
        UncoveredPit newPit = new UncoveredPit(pit);
        newPit.onTimeReached += () => {
            return GameManager.ins.SpawnEnemy(pit.Item2);
        };
        uncoveredPits.Add(newPit);
    }

    private void Update()
    {
        foreach (var pit in uncoveredPits) { 
            pit.Update();
        }
    }
}
