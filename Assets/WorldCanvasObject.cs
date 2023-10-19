using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasObject : MonoBehaviour
{
    Camera mainCamera;
    RectTransform rectTransform;
    Vector3 originalPosition = Vector3.zero;
    void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        originalPosition = mainCamera.ScreenToWorldPoint(rectTransform.position);
        Debug.Log(originalPosition);
    }
    void Update()
    {
        rectTransform.position = mainCamera.WorldToScreenPoint(originalPosition - mainCamera.transform.position);
        Debug.Log(mainCamera.WorldToScreenPoint(originalPosition - mainCamera.transform.position) + " | " + (originalPosition - mainCamera.transform.position));
    }
}
