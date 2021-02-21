using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThemeButton : MonoBehaviour
{
    //public UIManager uiManager;
    public Image normalColorImage;
    public Image highlightColorImage;

    public ThemeColors themeColors;

    // Start is called before the first frame update
    void Start()
    {
        normalColorImage.color = themeColors.normalColor;
        highlightColorImage.color = themeColors.highlightedColor;

        /*EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClickDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry);*/
    }

    /*public void OnPointerClickDelegate(PointerEventData data)
    {
        uiManager.SetColor(themeColors);
    }*/
}
