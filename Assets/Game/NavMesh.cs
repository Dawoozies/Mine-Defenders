using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMesh : MonoBehaviour
{
    NavMeshSurface surface;
    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMeshAsync();
    }
}
