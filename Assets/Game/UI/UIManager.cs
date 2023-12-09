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
    #region UI ACTION DISPLAY VARIABLES
    public GameObject UI_Action_Display_Prefab;
    const int UI_ACTION_DISPLAY_POOL_SIZE = 30;
    private readonly List<UI_Action_Display> actionDisplayPool = new();
    private UI_Action_Display actionDisplay_FirstAvailable;
    #endregion
    #region RADIAL MENU VARIABLES
    public delegate (int, bool) RadialButtonPressed();
    public event RadialButtonPressed onRadialButtonPressed;
    public GameObject[] radialMenus;
    #endregion
    private void Start()
    {
        #region UI ACTION DISPLAY START
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
        #endregion
    }
    #region UI ACTION DISPLAY
    public void Generate_Action_Display()
    {
        UI_Action_Display newPoolObject =
            Instantiate(UI_Action_Display_Prefab, transform).GetComponent<UI_Action_Display>();

        newPoolObject.ReturnToPoolEvent += ConfigureDeactivated_Action_Display;

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
    public void Generate_Agent_Display()
    {

    }
    private void Update()
    {
        #region RADIAL MENU UPDATE
        if(onRadialButtonPressed != null)
        {
            foreach (var item in onRadialButtonPressed.GetInvocationList())
            {
                (int, bool) buttonArgs = ((int, bool))item?.DynamicInvoke();
                radialMenus[buttonArgs.Item1].SetActive(buttonArgs.Item2);
            }
            //(int, bool) buttonArgs = onRadialButtonPressed.Invoke();
            //for (int i = 0; i < radialMenus.Length; i++)
            //{
            //    bool menuOpen = buttonArgs.Item1 == i && buttonArgs.Item2;
            //    radialMenus[i].SetActive(menuOpen);
            //}
            onRadialButtonPressed = null;
        }
        #endregion
    }
}