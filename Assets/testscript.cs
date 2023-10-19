using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class testscript : MonoBehaviour
{
    public TextMeshProUGUI tm;
    PlayerControls inputActions;
    Vector2 touchScreenPos;
    private void OnEnable()
    {
        inputActions = new PlayerControls();
        inputActions.Player.Tap.performed += inputActions => touchScreenPos = Camera.main.ScreenToWorldPoint(inputActions.ReadValue<Vector2>());
        inputActions.Player.Tap.performed += inputActions => tm.text = inputActions.ReadValue<Vector2>().ToString();
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void Update()
    {
        //transform.position = new Vector3(touchScreenPos.x, touchScreenPos.y, 0);
    }
}
