using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgent
{
    NavigationOrder Order { get; set; }
    public void SetMovementOrder();
    public void ReserveCell();
}
