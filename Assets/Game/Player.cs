using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    Agent agent;
    PlayerControls input;
    Vector2 touchInput;
    Vector3 worldTouchInput;
    Camera mainCamera;

    SpriteRenderer spriteRenderer;
    public float animTime = 1f;
    public List<Sprite> mainSprites;
    int mainSpriteIndex;
    float time = 0;
    void OnEnable()
    {
        agent = GetComponent<Agent>();
        mainCamera = Camera.main;
        input = new PlayerControls();
        input.Player.Tap.performed += (input) => {
            //Grab touch input
            touchInput = input.ReadValue<Vector2>();
            worldTouchInput = mainCamera.ScreenToWorldPoint(touchInput);
            agent.MoveToWorldPoint(worldTouchInput);
        };
        //input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (mainSprites != null)
        {
            spriteRenderer.sprite = mainSprites[mainSpriteIndex];
            time += Time.deltaTime;
            if (time > animTime)
            {
                mainSpriteIndex = (mainSpriteIndex + 1) % mainSprites.Count;
                time = 0;
            }
        }

        HandleMining();
    }
    public bool IsMoving()
    {
        return agent.GetRemainingDistance() > 0.1f;
    }
    public Pickaxe pickaxe;
    public Vector3Int miningTargetCell;
    Vector3 miningTargetWorldPos;
    public bool mining;
    public float distanceToTarget;
    public void StartMining(Tilemap targetTilemap, Vector3Int miningTargetCell, TileBase targetTile)
    {
        this.miningTargetCell = miningTargetCell;
        miningTargetWorldPos = targetTilemap.GetCellCenterWorld(miningTargetCell);
        distanceToTarget = Vector3.Distance(miningTargetWorldPos, transform.position);
        mining = true;
        if(distanceToTarget < 0.95f)
        {
            //Then we are close enough to start mining
            pickaxe.ToggleMiningEffect();
        }
        else
        {
            agent.MoveToWorldPoint(miningTargetWorldPos);
        }
    }
    void HandleMining()
    {
        if(mining)
        {
            distanceToTarget = Vector3.Distance(miningTargetWorldPos, transform.position);
            if(!IsMoving() && distanceToTarget > 1f)
            {
                //We have failed to path find to the target
                //Or there is no path taking us close enough
                Debug.Log("Failed to get to target");
                mining = false;
            }
            if(distanceToTarget < 0.95f)
            {
                Debug.Log("We reached target");
                if (!pickaxe.animate)
                    pickaxe.ToggleMiningEffect();
            }
        }
    }
    public void ForceStopMining()
    {
        if(pickaxe.animate)
            pickaxe.ToggleMiningEffect();
    }
}