using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButton
{
    public bool OnTap(Vector2 screenPos);
    public void ButtonPressed();
}