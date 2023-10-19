using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MenuTest : MonoBehaviour
{
    Camera mainCamera;
    Vector2 screenSpace;
    Vector3 worldSpace;
    public TextMeshProUGUI textmesh;
    RectTransform rectTransform;
    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        worldSpace = transform.position;
        screenSpace = mainCamera.WorldToScreenPoint(worldSpace);
        textmesh.text = screenSpace.ToString();
    }
}
