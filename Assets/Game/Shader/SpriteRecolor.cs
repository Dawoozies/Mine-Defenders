using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpriteRecolor : MonoBehaviour
{
    public bool recolorActive;
    public RecolorData recolorData;
    Image image;
    SpriteRenderer spriteRenderer;
    Material instancedMaterial;
    private void Start()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (image != null)
        {
            instancedMaterial = new Material(image.material);
            image.material = instancedMaterial;
        }
        if (spriteRenderer != null)
        {
            instancedMaterial = new Material(spriteRenderer.material);
            spriteRenderer.material = instancedMaterial;
        }
    }
    void Update()
    {
        if (image != null) ImageUpdate();
        if (spriteRenderer != null) SpriteRendererUpdate();
    }
    void ImageUpdate()
    {
        image.color = Color.white;
        image.material.SetFloat("_RecolorActive", recolorActive ? 1 : 0);
        image.material.mainTexture = image.sprite.texture;
        if (recolorData == null)
            return;
        if (recolorData.colorPairs == null || recolorData.colorPairs.Count == 0)
            return;
        for (int i = 0; i < recolorData.colorPairs.Count; i++)
        {
            image.material.SetColor($"_Target{i + 1}", recolorData.colorPairs[i].target);
            image.material.SetColor($"_Color{i + 1}", recolorData.colorPairs[i].color);
        }
    }
    void SpriteRendererUpdate()
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.material.SetFloat("_RecolorActive", recolorActive ? 1 : 0);
        spriteRenderer.material.mainTexture = spriteRenderer.sprite.texture;
        if (recolorData == null)
            return;
        if (recolorData.colorPairs == null || recolorData.colorPairs.Count == 0)
            return;
        for (int i = 0; i < recolorData.colorPairs.Count; i++)
        {
            spriteRenderer.material.SetColor($"_Target{i + 1}", recolorData.colorPairs[i].target);
            spriteRenderer.material.SetColor($"_Color{i + 1}", recolorData.colorPairs[i].color);
        }
    }
}
