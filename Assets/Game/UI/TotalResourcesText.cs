using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TotalResourcesText : MonoBehaviour
{
    TextAnimator textAnimator;
    private void Start()
    {
        textAnimator = GetComponent<TextAnimator>();
        GameManager.onPlayerLootPickup += ActivatePickupText;
    }
    void ActivatePickupText(string lootName, int amount, int totalAmount)
    {
        textAnimator.PlayAnimation("CountUp", totalAmount.ToString(), false, true);
    }
}
