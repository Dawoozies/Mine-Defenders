using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    public ObjectAnimator[] hands;
    void Start()
    {
        foreach (var h in hands)
        {
            h.PlayAnimation("Run");
        }
    }
}
