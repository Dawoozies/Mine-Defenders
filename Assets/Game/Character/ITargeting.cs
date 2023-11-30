using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargeting
{
    //Who am I targeting?
    //Who is targeting me?
    public TargetingArgs args { get; }
    public Vector3 worldPos { get; }
    public Vector3Int cellPos { get; }
    public void UpdateTarget(List<ITargeting> potentialTargets);
    void SetTarget(ITargeting newTarget);
}
public class TargetingArgs
{
    public ITargeting target;
    public List<ITargeting> targetedBy;
}