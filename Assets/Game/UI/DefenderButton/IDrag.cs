using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDrag
{
    public void OnDragStart();
    public void WhileDrag();
    public void OnDragEnd();
    public bool CheckDragAllowed();
}
