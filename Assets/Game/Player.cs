using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        input.Enable();
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
    }
}
