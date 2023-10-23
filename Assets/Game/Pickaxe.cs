using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : MonoBehaviour
{
    SpriteRenderer pickaxeRenderer;
    public List<Vector3> positions;
    public List<Vector3> rotations;
    List<Quaternion> trueRotations;
    public List<InterpolationType> interpolationTypes;
    public bool animate;
    public float time;
    public int currentIndex;
    public int targetIndex;
    public float timeMultiplier;
    private void Start()
    {
        pickaxeRenderer = GetComponent<SpriteRenderer>();
        targetIndex = (currentIndex + 1) % positions.Count;
        trueRotations = new List<Quaternion>();
        for (int i = 0; i < rotations.Count; i++)
        {
            trueRotations.Add(Quaternion.Euler(rotations[i]));
        }
    }
    void Update()
    {
        if (!animate)
            return;

        time += Time.deltaTime * timeMultiplier;
        if (time > 1)
        {
            currentIndex = (currentIndex + 1) % positions.Count;
            targetIndex = (currentIndex + 1) % positions.Count;
            time = 0;
        }
        transform.localPosition = Interpolation.Interpolate(positions[currentIndex], positions[targetIndex], time, interpolationTypes[currentIndex]);
        transform.localRotation = Interpolation.Interpolate(trueRotations[currentIndex], trueRotations[targetIndex], time, interpolationTypes[currentIndex]);
    }
    public void ToggleMiningEffect()
    {
        animate = !animate;
        if(animate)
        {
            currentIndex = 0;
            targetIndex = 1;
            time = 0;
            pickaxeRenderer.color = Color.white;
        }
        else
        {
            pickaxeRenderer.color = Color.clear;
        }
    }
}