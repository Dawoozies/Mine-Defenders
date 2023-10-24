using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testAnimation : MonoBehaviour
{
    public RectTransform rectTransform;
    void Update()
    {
        //rectTransform.sizeDelta = rectTransform.sizeDelta + new Vector2(0, Time.deltaTime);
        //rectTransform.anchoredPosition = rectTransform.anchoredPosition - new Vector2(0, Time.deltaTime/2);
        Debug.Log(rectTransform.sizeDelta.ToString());
        Debug.Log(rectTransform.anchoredPosition.ToString());
    }
}
