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
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickaxe = GetComponentInChildren<Pickaxe>();
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
    //Mining
    Pickaxe pickaxe;
    Vector3Int miningTargetCell;
    Vector3 miningTargetWorldPos;
    bool mining;
    float distanceToTarget;
    public void StartMining(Tilemap targetTilemap, Vector3Int miningTargetCell, TileBase targetTile)
    {
        this.miningTargetCell = miningTargetCell;
        miningTargetWorldPos = targetTilemap.GetCellCenterWorld(miningTargetCell);
        distanceToTarget = Vector3.Distance(miningTargetWorldPos, transform.position);
        mining = true;
        if(distanceToTarget < 0.95f)
        {
            //Then we are close enough to start mining
            pickaxe.ToggleMiningEffect(true);
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
                pickaxe.ToggleMiningEffect(true);
            }
        }
    }
    public void ForceStopMining()
    {
        pickaxe.ToggleMiningEffect(false);
    }
}