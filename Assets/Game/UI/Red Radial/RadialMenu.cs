using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RadialMenu : MonoBehaviour
{
    public GameObject closedMenu;
    public GameObject openedMenu;
    public Image toggleButtonImage;
    public SpriteAnimator menuAnimator;
    public ObjectAnimator shakeXAnimator;
    public ObjectAnimator shakeYAnimator;
    public Sprite openButtonSprite;
    public Sprite closeButtonSprite;
    bool menuOpen;
    public GameObject[] buttonObjects;
    RadialButton[] radialButtons;

    private void Start()
    {
        menuAnimator.EndFrameEvent += OnMenuOpening;
        List<RadialButton> buttons = new List<RadialButton>();
        foreach (GameObject buttonObject in buttonObjects)
        {
            buttons.Add(buttonObject.GetComponent<RadialButton>());
        }
        radialButtons = buttons.ToArray();
    }

    public void ToggleMenu()
    {
        menuOpen = !menuOpen;
        Debug.Log($"Menu open = {menuOpen}");
        //swap button sprite
        toggleButtonImage.sprite = menuOpen ? closeButtonSprite : openButtonSprite;
        if(menuOpen)
        {
            //Then we just opened the menu
            openedMenu.SetActive(true);
            closedMenu.SetActive(false);
            menuAnimator.PlayAnimation("Open");
        }
        else
        {
            openedMenu.SetActive(false);
            closedMenu.SetActive(true);
            foreach (GameObject buttonObject in buttonObjects)
            {
                buttonObject.SetActive(false);
            }
        }
    }
    void OnMenuOpening(int frame)
    {
        if (frame < 4)
        {
            buttonObjects[frame].SetActive(true);
            shakeXAnimator.PlayAnimation("ShakeX");
            shakeYAnimator.PlayAnimation("ShakeY");
        }
        if(frame == 3)
        {
            //Then we stop the animation here
            menuAnimator.StopAtNextFrame();
        }
    }
    public void ButtonPressed(RadialButton pressedButton)
    {
        foreach (RadialButton radialButton in radialButtons)
        {
            radialButton.ToggleButton(pressedButton);
        }
    }
}
