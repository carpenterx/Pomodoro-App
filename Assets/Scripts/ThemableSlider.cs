using UnityEngine;
using UnityEngine.UI;

public class ThemableSlider : MonoBehaviour
{
    public Image fillImage;
    public Image handleImage;

    private Color normalColor = Color.blue;
    private Color highlightedColor = Color.green;

    public void ChangeColors(ColorBlock colorBlock)
    {
        normalColor = colorBlock.normalColor;
        highlightedColor = colorBlock.highlightedColor;
        SetColors();
    }

    private void SetColors()
    {
        handleImage.color = normalColor;
        fillImage.color = highlightedColor;
    }
}
