using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Heart : MonoBehaviour
{
    public GameObject imagePrefab;
    public HeartPalette palette;
    public Sprite[] noMaskSprites;
    public Sprite[] maskSprites;
    public Transform mask;
    public Transform noMask;
    public TextMeshProUGUI valueText;
    public int value;
    public int maxValue;
    public int minValue;
    public float valuePercentage;
    RectTransform mainRect;
    Vector2 mainSizeDelta { get { return mainRect.sizeDelta; } }
    RectTransform maskRect;
    public RectTransform debugRect;
    public bool activateHitEffect;
    private void Start()
    {
        mainRect = GetComponent<RectTransform>();
        maskRect = mask.GetComponent<RectTransform>();
        for (int i = 0; i < noMaskSprites.Length; i++) 
        {
            RectTransform rectTransform = Instantiate(imagePrefab, noMask).GetComponent<RectTransform>();
            Image image = rectTransform.GetComponent<Image>();
            image.sprite = noMaskSprites[i];
            image.color = palette.noMaskColors[i];
            rectTransform.anchorMin = Vector2.right * 0.5f;
            rectTransform.anchorMax = Vector2.right * 0.5f;
            rectTransform.sizeDelta = Vector2.one * 100f;
            rectTransform.anchoredPosition = new Vector2(0, 50f);
        }
        for (int i = 0; i < maskSprites.Length; i++)
        {
            RectTransform rectTransform = Instantiate(imagePrefab, mask).GetComponent<RectTransform>();
            Image image = rectTransform.GetComponent<Image>();
            image.sprite = maskSprites[i];
            image.color = palette.maskColors[i];
            rectTransform.anchorMin = Vector2.right * 0.5f;
            rectTransform.anchorMax = Vector2.right * 0.5f;
            rectTransform.sizeDelta = Vector2.one * 100f;
            rectTransform.anchoredPosition = new Vector2(0, 50f);
        }
    }
    private void Update()
    {
        if (maxValue - minValue == 0)
            return;
        valuePercentage = (Mathf.Abs((float)(value - minValue)) / Mathf.Abs((float)(maxValue - minValue)));
        maskRect.sizeDelta = new Vector2(mainRect.sizeDelta.x, mainRect.sizeDelta.y * valuePercentage);
        maskRect.anchoredPosition = new Vector2(0f, maskRect.sizeDelta.y / 2f);
        valueText.text = value.ToString();
        debugRect.sizeDelta = valueText.GetRenderedValues(true);
        if(activateHitEffect)
        {
            activateHitEffect = false;
        }
    }
}
