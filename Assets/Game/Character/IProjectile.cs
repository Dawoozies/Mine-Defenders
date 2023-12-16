using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    public Vector3 firePos { get; }
    public void ReadyProjectile();
    public void ShootProjectile();
}
