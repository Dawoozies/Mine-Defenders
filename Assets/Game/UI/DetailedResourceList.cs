using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class DetailedResourceList : MonoBehaviour
{
    TextAnimator[] resourceTextList;
    public void Open()
    {
        if(resourceTextList == null)
            resourceTextList = GetComponentsInChildren<TextAnimator>();

        List<string> lootStrings = GameManager.ins.PlayerLootStringList();
        if (lootStrings == null || lootStrings.Count == 0)
            return;

        for(int i = 0; i < lootStrings.Count; i++)
        {
            resourceTextList[i].PlayAnimation("OnOpen", lootStrings[i], false, true);
        }
    }
    public void Close()
    {
        if (resourceTextList == null)
            resourceTextList = GetComponentsInChildren<TextAnimator>();

        List<string> lootStrings = GameManager.ins.PlayerLootStringList();
        if (lootStrings == null || lootStrings.Count == 0)
            return;

        for (int i = 0; i < lootStrings.Count; i++)
        {
            resourceTextList[i].PlayAnimation("OnClose", lootStrings[i], false, true);
        }
    }
}