using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Recolor Data")]
public class RecolorData : ScriptableObject
{
    public Sprite referenceSprite;
    public List<ColorPair> colorPairs;
}
[Serializable]
public class ColorPair
{
    public Color target;
    public Color color;
}