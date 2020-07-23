using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public GameObject settingsHolder;
    public UIManager uiManager;
    public TimeKeeper timeKeeper;
    public AudioMixer audioMixer;

    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Slider volumeSlider;

    private Resolution[] resolutions;

    private void Start()
    {
        settingsHolder.SetActive(false);
        // it is possible that the resolutions returned have duplicate values because of multiple screen refresh rates
        // fix can be found here:
        // https://answers.unity.com/questions/1463609/screenresolutions-returning-duplicates.html
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> optionsList = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            optionsList.Add(option);

            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(optionsList);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
        // gets the master volume and aligns it to the volume slider
        // in theory, it should reverse the formula used for the volume scaling
        float volumeValue;
        audioMixer.GetFloat("masterVolume", out volumeValue);
        volumeSlider.value = Mathf.Pow(10f, volumeValue / 20);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(settingsHolder.activeSelf)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
        }
    }

    public void ShowSettings()
    {
        uiManager.UpdateSettingsUI();
        settingsHolder.SetActive(true);
        timeKeeper.MenuPause();
    }

    public void HideSettings()
    {
        settingsHolder.SetActive(false);
        timeKeeper.MenuResume();
    }

    public void SetVolume(float volume)
    {
        //This calculates the volume properly since it does not scale in a linear fashion
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
