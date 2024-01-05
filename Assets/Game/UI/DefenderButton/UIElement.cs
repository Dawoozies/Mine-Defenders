using System;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class UIElement
{
    public GameObject gameObject;
    [HideInInspector]
    public RectTransform rectTransform;
    [HideInInspector]
    public Image image;
    public void SetUp()
    {
        if (gameObject == null)
            return;
        rectTransform = gameObject.GetComponent<RectTransform>();
        image = gameObject.GetComponent<Image>();
    }
}
