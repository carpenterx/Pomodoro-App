﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Themable : MonoBehaviour
{
    //public string label;
    
    public Image borderImage;
    //public Text text;

    //public ThemeColors themeColors;
    private Color normalColor = Color.blue;
    private Color highlightedColor = Color.green;

    void Start()
    {
        /*if(text.text != label)
        {
            text.text = label;
        }*/
        
        SetBorderColor(normalColor);

        EventTrigger mouseOverTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry mouseOverEntry = new EventTrigger.Entry();
        mouseOverEntry.eventID = EventTriggerType.PointerEnter;
        mouseOverEntry.callback.AddListener(delegate { SetBorderColor(highlightedColor); });
        mouseOverTrigger.triggers.Add(mouseOverEntry);

        EventTrigger mouseOutTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry mouseOutEntry = new EventTrigger.Entry();
        mouseOutEntry.eventID = EventTriggerType.PointerExit;
        mouseOutEntry.callback.AddListener(delegate { SetBorderColor(normalColor); });
        mouseOutTrigger.triggers.Add(mouseOutEntry);
    }

    private void SetBorderColor(Color color)
    {
        borderImage.color = color;
    }

    public void ChangeColors(ColorBlock colorBlock)
    {
        normalColor = colorBlock.normalColor;
        highlightedColor = colorBlock.highlightedColor;
        SetBorderColor(normalColor);
    }
}
