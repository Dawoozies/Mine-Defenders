using System.Collections.Generic;
using UnityEngine;

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
            nextSpawn = Time.time + 2f;
        }

        public void Update() {
            if (Time.time > nextSpawn) {
                onTimeReached?.Invoke();
                nextSpawn = Time.time + 2f;
            }
        }
    }
    List<UncoveredPit> uncoveredPits;
    private void Update()
    {
        return;
        foreach (var pit in uncoveredPits) { 
            pit.Update();
        }
    }
}
