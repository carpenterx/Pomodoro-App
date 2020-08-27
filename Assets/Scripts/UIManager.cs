using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI PomodoroNameText;
    public InputField profileNameInput;
    public InputField pomNameInput;
    public InputField pomSecondsInput;
    public Button profilePrefab;
    public Button pomodoroPrefab;
    public ScrollRect profilesScrollViewer;
    public ScrollRect pomodorosScrollViewer;
    public List<Button> themableButtonsList;
    public Dropdown soundsDropdown;
    public AudioSource audioSource;
    public List<AudioClip> audioClips;

    public ThemeColors redThemeColors;
    public ThemeColors yellowThemeColors;
    public ThemeColors greenThemeColors;
    public ThemeColors tealThemeColors;
    public ThemeColors purpleThemeColors;
    public ThemeColors blueThemeColors;

    private List<string> audioClipNames = new List<string>();

    private ColorBlock currentColors;

    private TimeKeeper timeKeeper;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();

        foreach (AudioClip clip in audioClips)
        {
            audioClipNames.Add(clip.name);
        }

        soundsDropdown.ClearOptions();
        soundsDropdown.AddOptions(audioClipNames);

        timeKeeper = gameObject.GetComponent<TimeKeeper>();
        // if there is no save data, set default values for app data
        ProfileData loadedData = (ProfileData)JsonSave.Load(ProfileData.Current);
        if(loadedData != null)
        {
            ProfileData.Current = loadedData;
            // the pomodoros list should always have a pomodoro in it
            // here we just generate a default pomodoro and add it to the current profile
            AddDefaultPomodoroFallback();
        }
        else
        {
            ProfileData.Current.ButtonNormalColor = "#16A085";
            ProfileData.Current.ButtonHighlightedColor = "#1ABC9C";
            // the pomodoros list should always have a pomodoro in it
            // here we just generate a default pomodoro and add it to the current profile
            AddDefaultPomodoroFallback();

            //ProfileData.ChangeFileName(ProfileData.DefaultFileName);
            JsonSave.Save(ProfileData.Current);
        }

        timeKeeper.LoadCurrentPomodoro();
        LoadProfileColors();

        GenerateProfilesDisplay();
        GeneratePomodorosDisplay();

        #region Some test code
        /*// test code
        string colorPresetPath = "Assets/Editor/App Colors.colors";
        UnityEngine.Object presetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(colorPresetPath);
        SerializedObject so = new SerializedObject(presetObject);
        SerializedProperty presets = so.FindProperty("m_Presets");
        SerializedProperty firstElement = presets.GetArrayElementAtIndex(0);
        Debug.Log(firstElement.FindPropertyRelative("m_Name").stringValue);
        //Debug.Log(firstElement.FindPropertyRelative("m_Color").colorValue);*/
        #endregion
    }

    public void UpdateSettingsUI()
    {
        UpdatePomodorosPlayIcon();
    }

    public void SelectPomodoroPlayIndex()
    {
        if(ProfileData.SelectedPomodoroIndex != -1)
        {
            ProfileData.Current.PomodoroPlayIndex = ProfileData.SelectedPomodoroIndex;
            timeKeeper.LoadCurrentPomodoro();
            UpdatePomodorosPlayIcon();
        }
    }

    private void UpdatePomodorosPlayIcon()
    {
        Button[] buttons = pomodorosScrollViewer.content.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Image[] images = buttons[i].GetComponentsInChildren<Image>();
            if (i == ProfileData.Current.PomodoroPlayIndex)
            {
                images[1].enabled = true;
            }
            else
            {
                images[1].enabled = false;
            }
        }
    }

    private void AddDefaultPomodoroFallback()
    {
        if (ProfileData.Current.Pomodoros.Count == 0)
        {
            Pomodoro defaultPomodoro = new Pomodoro("default", 900, "victory");
            ProfileData.Current.Pomodoros.Add(defaultPomodoro);
        }
    }

    public void PlayEndSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.PlayOneShot(audioClips[soundsDropdown.value]);
    }

    public void PlayEndSound(string soundName)
    {
        AudioClip clip = audioClips.Find(c => c.name == soundName);
        if(clip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayClipSound()
    {
        if(ProfileData.SelectedPomodoroIndex != -1)
        {
            Pomodoro pomodoro = ProfileData.Current.Pomodoros[ProfileData.SelectedPomodoroIndex];
            string soundName = pomodoro.SoundName;
            AudioClip clip = audioClips.Find(c => c.name == soundName);
            if (clip != null)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private void GenerateProfilesDisplay()
    {
        foreach (Transform button in profilesScrollViewer.content)
        {
            Destroy(button.gameObject);
        }
        if(Directory.Exists(ProfileData.FolderName))
        {
            string[] profiles = Directory.GetFiles(ProfileData.FolderName, "*" + ProfileData.FileExtension);
            for (int i = 0; i < profiles.Length; i++)
            {
                Button button = Instantiate(profilePrefab);
                button.colors = currentColors;
                button.transform.SetParent(profilesScrollViewer.content.transform, false);
                string fileName = Path.GetFileNameWithoutExtension(profiles[i]);
                button.GetComponent<Button>().onClick.AddListener(delegate { UpdateProfileText(fileName); });
                button.GetComponentInChildren<Text>().text = fileName;
            }
        }
    }

    public void UpdateProfileText(string fileName)
    {
        profileNameInput.text = fileName;
    }

    private void UpdateProfileListColors()
    {
        Button[] buttons = profilesScrollViewer.content.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].colors = currentColors;
        }
    }

    public void GeneratePomodorosDisplay()
    {
        ClearPomodorosList();

        for (int i = 0; i < ProfileData.Current.Pomodoros.Count; i++)
        {
            // this sends the current value of i to the delegate function instead of the value at the end of the loop
            var index = i;
            CreatePomodoroPrefab(index);
        }
    }

    private void CreatePomodoroPrefab(int index)
    {
        Button button = Instantiate(pomodoroPrefab);
        button.colors = currentColors;
        button.transform.SetParent(pomodorosScrollViewer.content.transform, false);
        button.GetComponent<Button>().onClick.AddListener(delegate { SelectPomodoro(index); });
        Text[] textBoxes = button.GetComponentsInChildren<Text>();
        textBoxes[0].text = ProfileData.Current.Pomodoros[index].Name;
        textBoxes[2].text = ProfileData.Current.Pomodoros[index].Duration.ToString();
        textBoxes[4].text = ProfileData.Current.Pomodoros[index].SoundName;
        Image[] images = button.GetComponentsInChildren<Image>();
        if(index == ProfileData.Current.PomodoroPlayIndex)
        {
            images[1].enabled = true;
        }
        else
        {
            images[1].enabled = false;
        }
    }

    private void SelectPomodoro(int index)
    {
        ProfileData.SetSelectedPomodoro(index);
        pomNameInput.text = ProfileData.Current.Pomodoros[index].Name;
        pomSecondsInput.text = ProfileData.Current.Pomodoros[index].Duration.ToString();
        int soundIndex = audioClips.FindIndex(c => c.name == ProfileData.Current.Pomodoros[index].SoundName);
        soundsDropdown.value = soundIndex;
    }

    private void ClearPomodorosList()
    {
        foreach (Transform button in pomodorosScrollViewer.content)
        {
            Destroy(button.gameObject);
        }
    }

    public void UpdatePomodorosListColors()
    {
        Button[] buttons = pomodorosScrollViewer.content.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].colors = currentColors;
        }
    }

    public void AddPomodoroToList()
    {
        ProfileData.Current.Pomodoros.Add(new Pomodoro(pomNameInput.text, pomSecondsInput.text, soundsDropdown.options[soundsDropdown.value].text));

        int pomodoroInd = ProfileData.Current.Pomodoros.Count - 1;
        CreatePomodoroPrefab(pomodoroInd);
        RefreshSelectionListeners();
    }

    public void RemovePomodoroFromList()
    {
        if(ProfileData.Current.Pomodoros.Count > 1)
        {
            int index = ProfileData.SelectedPomodoroIndex;
            if (index >= 0)
            {
                ProfileData.Current.Pomodoros.RemoveAt(index);
                if (index == 0)
                {
                    timeKeeper.LoadCurrentPomodoro();
                }

                RemovePomodoroPrefabAt(index);
                ProfileData.ResetSelectedPomodoro();
                if(ProfileData.Current.PomodoroPlayIndex == index)
                {
                    ProfileData.Current.PomodoroPlayIndex = 0;
                    UpdatePomodorosPlayIcon();
                }
            }
        }
    }

    public void RemovePomodoroPrefabAt(int index)
    {
        // code that deletes the child of a gameobject at a known index
        Transform child = pomodorosScrollViewer.content.GetChild(index);
        // unparent the child first, since it seems like it is not getting removed immediately
        child.transform.SetParent(null, false);
        Destroy(child.gameObject);
        RefreshSelectionListeners();
    }

    public void MovePomodoroUp()
    {
        // there need to be more than 2 pomodoros, a pomodoro needs to be selected, and the selected pomodoro needs to not be the first element
        int selectedIndex = ProfileData.SelectedPomodoroIndex;
        if (selectedIndex > 0 && ProfileData.Current.Pomodoros.Count > 1)
        {
            Pomodoro tempPomodoro = ProfileData.Current.Pomodoros[selectedIndex];
            ProfileData.Current.Pomodoros[selectedIndex] = ProfileData.Current.Pomodoros[selectedIndex - 1];
            ProfileData.Current.Pomodoros[selectedIndex - 1] = tempPomodoro;
            UpdatePomodoroPrefabText(selectedIndex);
            UpdatePomodoroPrefabText(selectedIndex - 1);
            if(ProfileData.Current.PomodoroPlayIndex == selectedIndex)
            {
                ProfileData.Current.PomodoroPlayIndex = selectedIndex - 1;
            }
            else if(ProfileData.Current.PomodoroPlayIndex == selectedIndex - 1)
            {
                ProfileData.Current.PomodoroPlayIndex = selectedIndex;
            }
            UpdatePomodorosPlayIcon();
        }
    }

    public void MovePomodoroDown()
    {
        // there need to be more than 2 pomodoros, a pomodoro needs to be selected, and the selected pomodoro needs to not be the last element
        int selectedIndex = ProfileData.SelectedPomodoroIndex;
        int pomoCount = ProfileData.Current.Pomodoros.Count;
        if (selectedIndex > -1 && selectedIndex < pomoCount - 1 && pomoCount > 1)
        {
            Pomodoro tempPomodoro = ProfileData.Current.Pomodoros[selectedIndex];
            ProfileData.Current.Pomodoros[selectedIndex] = ProfileData.Current.Pomodoros[selectedIndex + 1];
            ProfileData.Current.Pomodoros[selectedIndex + 1] = tempPomodoro;
            UpdatePomodoroPrefabText(selectedIndex);
            UpdatePomodoroPrefabText(selectedIndex + 1);
            if(ProfileData.Current.PomodoroPlayIndex == selectedIndex)
            {
                ProfileData.Current.PomodoroPlayIndex = selectedIndex + 1;
            } 
            else if(ProfileData.Current.PomodoroPlayIndex == selectedIndex + 1)
            {
                ProfileData.Current.PomodoroPlayIndex = selectedIndex;
            }
            UpdatePomodorosPlayIcon();
        }
    }

    public void RefreshSelectionListeners()
    {
        int pomodorosCount = pomodorosScrollViewer.content.childCount;
        for (int i = 0; i < pomodorosCount; i++)
        {
            Transform pomodoro = pomodorosScrollViewer.content.GetChild(i);
            Button button = pomodoro.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            // this sends the current value of i to the delegate function instead of the value at the end of the loop
            var index = i;
            button.onClick.AddListener(delegate { SelectPomodoro(index); });
        }
    }

    private void UpdatePomodoroPrefabText(int index)
    {
        Button button = pomodorosScrollViewer.content.GetChild(index).GetComponent<Button>();
        Text[] textBoxes = button.GetComponentsInChildren<Text>();
        textBoxes[0].text = ProfileData.Current.Pomodoros[index].Name;
        textBoxes[2].text = ProfileData.Current.Pomodoros[index].Duration.ToString();
        textBoxes[4].text = ProfileData.Current.Pomodoros[index].SoundName;
    }

    public void DisplayCurrentPomodoroName(string name)
    {
        PomodoroNameText.text = name;
    }

    public void SaveProfile()
    {
        if(profileNameInput.text != "")
        {
            ProfileData.ChangeFileName(profileNameInput.text);
            JsonSave.Save(ProfileData.Current);
            GenerateProfilesDisplay();
        }
    }

    public void LoadProfile()
    {
        if (profileNameInput.text != "")
        {
            ProfileData.ChangeFileName(profileNameInput.text);
            ProfileData loadedData = (ProfileData)JsonSave.Load(ProfileData.Current);
            if (loadedData != null)
            {
                ProfileData.Current = loadedData;
                AddDefaultPomodoroFallback();
                timeKeeper.LoadCurrentPomodoro();
                LoadProfileColors();

                UpdateProfileListColors();
                GeneratePomodorosDisplay();
            }
        }
    }

    private void OnApplicationQuit()
    {
        // the data gets saved to the last filepath, so I need to reset it to default first before autosaving
        ProfileData.ChangeFileName(ProfileData.DefaultFileName);
        JsonSave.Save(ProfileData.Current);
    }

    public void SetRedColor()
    {
        //SetColor("#c0392b", "#e74c3c");
        SetColor(redThemeColors);
    }

    public void SetYellowColor()
    {
        //SetColor("#f39c12", "#f1c40f");
        SetColor(yellowThemeColors);
    }

    public void SetGreenColor()
    {
        //SetColor("#27ae60", "#2ecc71");
        SetColor(greenThemeColors);
    }

    public void SetTealColor()
    {
        //SetColor("#16A085", "#1ABC9C");
        SetColor(tealThemeColors);
    }

    public void SetPurpleColor()
    {
        //SetColor("#8E44AD", "#9B59B6");
        SetColor(purpleThemeColors);
    }

    public void SetBlueColor()
    {
        //SetColor("#2980b9", "#3498db");
        SetColor(blueThemeColors);
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

    private void SetColor(string normalColor, string highlightedColor)
    {
        ProfileData.Current.ButtonNormalColor = normalColor;
        ProfileData.Current.ButtonHighlightedColor = highlightedColor;
        ChangeCurrentColor(normalColor, highlightedColor);
    }

    private void SetColor(ThemeColors themeColors)
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
        UpdatePomodorosListColors();
        UpdateProfileListColors();
        UpdateThemableButtonsColors();
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
