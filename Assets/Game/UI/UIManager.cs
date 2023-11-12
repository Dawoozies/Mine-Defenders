using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static UIManager ins;
    private void Awake()
    {
        ins = this;
    }
    public GameObject UI_Action_Display_Prefab;
    const int UI_ACTION_DISPLAY_POOL_SIZE = 30;
    private readonly List<UI_Action_Display> actionDisplayPool = new();
    private UI_Action_Display actionDisplay_FirstAvailable;
    private void Start()
    {
        for (int i = 0; i < UI_ACTION_DISPLAY_POOL_SIZE; i++)
        {
            Generate_Action_Display();
        }
        actionDisplay_FirstAvailable = actionDisplayPool[0];
        for (int i = 0; i < actionDisplayPool.Count - 1; i++)
        {
            actionDisplayPool[i].next = actionDisplayPool[i + 1];
        }
        actionDisplayPool[^1].next = null;
    }
    #region UI ACTION DISPLAY
    public void Generate_Action_Display()
    {
        UI_Action_Display newPoolObject =
            Instantiate(UI_Action_Display_Prefab, transform).GetComponent<UI_Action_Display>();

        newPoolObject.onTimeDone += ConfigureDeactivated_Action_Display;

        newPoolObject.gameObject.SetActive(false);

        actionDisplayPool.Add(newPoolObject);
    }
    public void ConfigureDeactivated_Action_Display(UI_Action_Display deactivatedObj)
    {
        deactivatedObj.next = actionDisplay_FirstAvailable;
        actionDisplay_FirstAvailable = deactivatedObj;
    }
    public UI_Action_Display Get_Action_Display()
    {
        if(actionDisplay_FirstAvailable == null)
        {
            if(actionDisplayPool.Count < UI_ACTION_DISPLAY_POOL_SIZE)
            {
                Generate_Action_Display();
                UI_Action_Display lastActionDisplay = actionDisplayPool[^1];
                ConfigureDeactivated_Action_Display(lastActionDisplay);
            }
            else
            {
                return null;
            }
        }
        UI_Action_Display newActionDisplay = actionDisplay_FirstAvailable;
        actionDisplay_FirstAvailable = newActionDisplay.next;
        newActionDisplay.gameObject.SetActive(true);
        return newActionDisplay;
    }
    #endregion
}