using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPlayer : MonoBehaviour
{
    CharacterAgent agent;
    void Start()
    {
        GridInteraction.GridInteractEvent += PlayerTap;
        agent = GetComponent<CharacterAgent>();
    }
    void PlayerTap(GridInteractArgs args)
    {
        agent.MovementOrder("MoveToPosition", args.CellWorldPosition);
    }
}
