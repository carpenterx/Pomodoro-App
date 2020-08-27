using UnityEngine;
using UnityEngine.UI;

public class ThemeButton : MonoBehaviour
{
    public Image normalColorImage;
    public Image highlightColorImage;

    public ThemeColors themeColors;
    // Start is called before the first frame update
    void Start()
    {
        normalColorImage.color = themeColors.normalColor;
        highlightColorImage.color = themeColors.highlightedColor;
    }
}
