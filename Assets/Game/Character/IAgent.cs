using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgent
{
    public AgentType AgentType { get; }
    public void SetMovementOrder(CellData cellData);
    public void ReserveCell(CellData cellData);
}
public enum AgentType
{
    Player = 0,
    Enemy = 1,
    Defender = 2,
}