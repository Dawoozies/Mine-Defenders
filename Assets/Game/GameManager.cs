using NavMeshPlus.Components;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    LevelGenerator levelGenerator;
    NavMeshSurface navMeshSurface;
    CharacterGenerator characterGenerator;
    //Player character
    //Defender character
    //Enemy character
    void Awake()
    {
        levelGenerator = GetComponentInChildren<LevelGenerator>();
        navMeshSurface = GetComponentInChildren<NavMeshSurface>();
        characterGenerator = GetComponentInChildren<CharacterGenerator>();
    }
    void Start()
    {
        levelGenerator.ManagedStart();
        navMeshSurface.BuildNavMesh();
        characterGenerator.ManagedStart();
    }
    void Update()
    {
        
    }
}
