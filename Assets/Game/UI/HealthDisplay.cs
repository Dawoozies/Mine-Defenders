using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    //this won't be just health display
    //it will be a value threshold display
    //can be used for mining progress as well
    //can be used for showing attack charge
    public List<Sprite> containerSprites;
    public int containers;
    public int value;
    private void Start()
    {
        if (containerSprites == null || containerSprites.Count == 0)
            return;
        if (containers <= 0)
            return;
        int fillValue = containerSprites.Count - 1;
        for (int i = 0; i < containers; i++)
        {
            int containerValue = value - i * fillValue;
            containerValue = Mathf.Clamp(containerValue, 0, fillValue);
            Debug.Log($"Container {i} has value = {containerValue}");
        }
    }
}