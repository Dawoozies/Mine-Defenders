using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager ins;

    LevelGenerator levelGenerator;
    NavMeshSurface navMeshSurface;
    CharacterGenerator characterGenerator;

    Camera mainCamera;
    ObjectAnimator cameraAnimator;

    //Player character
    //Defender character
    //Enemy character
    void Awake()
    {
        ins = this;
        levelGenerator = GetComponentInChildren<LevelGenerator>();
        navMeshSurface = GetComponentInChildren<NavMeshSurface>();
        characterGenerator = GetComponentInChildren<CharacterGenerator>();
        mainCamera = Camera.main;
        cameraAnimator = mainCamera.GetComponent<ObjectAnimator>();
    }
    void Start()
    {
        levelGenerator.ManagedStart();
        navMeshSurface.BuildNavMesh();
        characterGenerator.ManagedStart();
    }
    
    public delegate Vector3 PlayerPositionUpdated();
    public static event PlayerPositionUpdated OnPlayerPositionUpdated;
    void Update()
    {
        //This is where we should manage external "coupling"
        if(OnPlayerPositionUpdated != null)
        {
            Vector3 PlayerPosition = OnPlayerPositionUpdated.Invoke();
            CameraTrackAnimation(PlayerPosition);
            OnPlayerPositionUpdated = null;
        }
    }
    
    void CameraTrackAnimation(Vector3 targetPos)
    {
        targetPos.z = mainCamera.transform.position.z;
        ObjectAnimation trackingAnimation = new ObjectAnimation();
        trackingAnimation.animName = "PlayerTracking";
        trackingAnimation.frames = 2;
        trackingAnimation.positions = new List<Vector3> { mainCamera.transform.position, targetPos};
        trackingAnimation.interpolationTypes = new List<InterpolationType> { InterpolationType.EaseOutExp, InterpolationType.EaseOutExp};
        trackingAnimation.loop = false;

        cameraAnimator.CreateAndPlayAnimation(trackingAnimation);
    }
    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        worldPosition.z = 0;
        return levelGenerator.StoneTilemap.WorldToCell(worldPosition);
    }
    public Vector3 WorldToCellCenter(Vector3 worldPosition)
    {
        worldPosition.z = 0;
        return levelGenerator.StoneTilemap.GetCellCenterWorld(WorldToCell(worldPosition));
    }
}
