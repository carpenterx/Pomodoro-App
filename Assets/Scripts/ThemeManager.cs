﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    //public UIManager uiManager;

    [Space]
    public List<Button> themableButtonsList;

    [Space]
    public ScrollRect pomodorosScrollViewer;

    [Space]
    public List<ThemeButton> themeButtons;

    public ColorBlock currentColors;

    void Start()
    {
        foreach (ThemeButton themeButton in themeButtons)
        {
            EventTrigger trigger = themeButton.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(delegate { ChangeColor(themeButton.themeColors); });
            trigger.triggers.Add(entry);
        }
    }

    public void ChangeColor(ThemeColors colors)
    {
        SetColor(colors);
    }

    private Color GetColorFromHexString(string hexString)
    {
        Color color;
        ColorUtility.TryParseHtmlString(hexString, out color);
        return color;
    }

    public void LoadProfileColors()
    {
        ChangeCurrentColor(ProfileData.Current.ButtonNormalColor, ProfileData.Current.ButtonHighlightedColor);
    }

    public void SetColor(ThemeColors themeColors)
    {
        ProfileData.Current.ButtonNormalColor = "#" + ColorUtility.ToHtmlStringRGB(themeColors.normalColor);
        ProfileData.Current.ButtonHighlightedColor = "#" + ColorUtility.ToHtmlStringRGB(themeColors.highlightedColor);
        ChangeCurrentColor(themeColors);
    }

    private void ChangeCurrentColor(string normalColor, string highlightedColor)
    {
        currentColors = BuildColorBlock(normalColor, highlightedColor);
        UpdateAllColors();
    }

    private void ChangeCurrentColor(ThemeColors themeColors)
    {
        currentColors = BuildColorBlock(themeColors);
        UpdateAllColors();
    }

    private void UpdateThemableButtonsColors()
    {
        foreach (Button button in themableButtonsList)
        {
            button.colors = currentColors;
        }
    }

    private void UpdateAllColors()
    {
        UpdateScrollviewChildrenColors(pomodorosScrollViewer);
        UpdateThemableButtonsColors();
    }

    private void UpdateScrollviewChildrenColors(ScrollRect parent)
    {
        Button[] buttons = parent.content.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].colors = currentColors;
        }
    }

    private ColorBlock BuildColorBlock(string normalColor, string highlightedColor)
    {
        ColorBlock colorBlock = ColorBlock.defaultColorBlock;
        colorBlock.normalColor = GetColorFromHexString(normalColor);
        colorBlock.highlightedColor = GetColorFromHexString(highlightedColor);
        return colorBlock;
    }

    private ColorBlock BuildColorBlock(ThemeColors themeColors)
    {
        ColorBlock colorBlock = ColorBlock.defaultColorBlock;
        colorBlock.normalColor = themeColors.normalColor;
        colorBlock.highlightedColor = themeColors.highlightedColor;
        return colorBlock;
    }
}
