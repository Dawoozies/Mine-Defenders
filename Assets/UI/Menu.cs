using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public Vector2 screen;
    private void Start()
    {
        screen = Camera.main.WorldToScreenPoint(Vector3.zero);
    }
    public void ShiftLeft()
    {
        Vector3 screenSpace = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
        screenSpace += new Vector3(-screen.x,0,0);
        Vector3 worldSpace = Camera.main.ScreenToWorldPoint(screenSpace);
        Camera.main.transform.position = worldSpace;
    }
    public void ShiftRight()
    {
        Vector3 screenSpace = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
        screenSpace += new Vector3(screen.x, 0, 0);
        Vector3 worldSpace = Camera.main.ScreenToWorldPoint(screenSpace);
        Camera.main.transform.position = worldSpace;
    }
}