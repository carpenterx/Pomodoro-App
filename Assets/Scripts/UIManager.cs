using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Audio;
using System.Globalization;

public class UIManager : MonoBehaviour
{
    public ThemeManager themeManager;
    public TextMeshProUGUI PomodoroNameText;
    public InputField profileNameInput;
    public InputField pomNameInput;
    public InputField pomMinutesInput;
    public InputField pomSecondsInput;
    public Button profilePrefab;
    public Button pomodoroPrefab;
    public ScrollRect pomodorosScrollViewer;
    //public List<Button> themableButtonsList;
    public Dropdown soundsDropdown;
    public Dropdown profilesDropdown;
    public BackgroundImage backgroundImage;
    public AudioSource audioSource;
    private List<AudioClip> audioClips = new List<AudioClip>();

    private List<string> audioClipNames = new List<string>();

    private TimeKeeper timeKeeper;
    public GameObject settingsHolder;
    public GameObject appHolder;
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public Text volumeValue;

    public Text profileNameText;

    private static readonly string filePrefix = "file://";
    private static string noSoundString = "No sound";
    private List<string> soundPathsList = new List<string>();

    private void Awake()
    {
        settingsHolder.SetActive(false);
        // gets the master volume and aligns it to the volume slider
        // in theory, it should reverse the formula used for the volume scaling
        /*float volumeValue;
        audioMixer.GetFloat("masterVolume", out volumeValue);
        volumeSlider.value = Mathf.Pow(10f, volumeValue / 20);*/

        audioSource = gameObject.GetComponent<AudioSource>();


        timeKeeper = gameObject.GetComponent<TimeKeeper>();
    }

    private void Start()
    {
        LoadDefaultProfile();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsHolder.activeSelf)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
        }
    }

    private void InitializeSoundsList()
    {
        soundPathsList = new List<string>();
        soundPathsList.Add(noSoundString);
        audioClips = new List<AudioClip>();
        audioClips.Add(null);
        audioClipNames = new List<string>();
        audioClipNames.Add(noSoundString);
        soundsDropdown.ClearOptions();
        soundsDropdown.AddOptions(audioClipNames);
    }

    public void ShowSettings()
    {
        UpdateSettingsUI();
        settingsHolder.SetActive(true);
        ChangeAppChildrenVisibility(false);
        timeKeeper.MenuPause();
    }

    public void HideSettings()
    {
        settingsHolder.SetActive(false);
        ChangeAppChildrenVisibility(true);
        timeKeeper.MenuResume();
    }

    private void ChangeAppChildrenVisibility(bool visibility)
    {
        foreach (Transform child in appHolder.transform)
        {
            child.gameObject.SetActive(visibility);
        }
    }

    public void UpdateVolume(float volume)
    {
        ProfileData.Current.Volume = volume.ToString(CultureInfo.InvariantCulture.NumberFormat);
        SetVolume(volume);
    }

    private void SetVolume(float volume)
    {
        //This calculates the volume properly since it does not scale in a linear fashion
        audioMixer.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
        volumeValue.text = Math.Round(volume * 100).ToString();
    }

    private void GenerateSoundsListFromPomodoros()
    {
        List<string> pomodoroPaths = new List<string>();
        foreach (Pomodoro pomodoro in ProfileData.Current.Pomodoros)
        {
            pomodoroPaths.Add(pomodoro.SoundPath);
        }
        List<string> uniquePaths = pomodoroPaths.Distinct().ToList();
        int noSoundIndex = uniquePaths.IndexOf(noSoundString);
        if(noSoundIndex != -1)
        {
            uniquePaths.RemoveAt(noSoundIndex);
        }
        LoadSoundPathsList(uniquePaths);
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
            timeKeeper.ResetCurrentTime();
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
        // the pomodoros list should always have a pomodoro in it
        // here we just generate a default pomodoro and add it to the current profile
        if (ProfileData.Current.Pomodoros.Count == 0)
        {
            Pomodoro defaultPomodoro = new Pomodoro("default", 900, noSoundString);
            ProfileData.Current.Pomodoros.Add(defaultPomodoro);
        }
    }

    public void PlayEndSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if(soundsDropdown.options[soundsDropdown.value].text != noSoundString)
        {
            audioSource.PlayOneShot(audioClips[soundsDropdown.value]);
        }
    }

    public void PlayEndSound(string soundName)
    {
        PlaySoundClip(soundName);
    }

    private void PlaySoundClip(string soundPath)
    {
        if (soundPath != noSoundString)
        {
            AudioClip clip = audioClips.Find(c => c != null && c.name == Path.GetFileNameWithoutExtension(soundPath));
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if(clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    public void BrowseToSounds()
    {
        var extenstions = new[] { new ExtensionFilter("Sound Files", "mp3", "ogg", "wav", "aif", "aiff") };
        List<string> selectedFiles = StandaloneFileBrowser.OpenFilePanel("Open Sound File", "", extenstions, true).ToList();
        if(selectedFiles.Count > 0)
        {
            LoadSoundPathsList(selectedFiles);
        }
    }

    private void LoadSoundPathsList(List<string> soundPaths)
    {
        AddSounds(soundPaths);
        int startCount = soundPathsList.Count;
        soundPathsList.AddRange(soundPaths);
        // pad the audioclips list before loading the audioclips
        audioClips.AddRange(new AudioClip[soundPaths.Count]);
        StartCoroutine(GetAudioClips(startCount));
    }

    private void AddSounds(List<string> soundPaths)
    {
        List<string> fileNamesList = new List<string>();
        for (int i = 0; i < soundPaths.Count; i++)
        {
            if (soundPaths[i] != noSoundString)
            {
                string fileName = Path.GetFileNameWithoutExtension(soundPaths[i]);
                fileNamesList.Add(fileName);
            }
        }
        soundsDropdown.AddOptions(fileNamesList);
    }

    private IEnumerator GetAudioClips(int startIndex)
    {
        int index = startIndex;
        while (index < soundPathsList.Count)
        {
            if(soundPathsList[index] != noSoundString)
            {
                string path = Path.Combine(filePrefix + soundPathsList[index]);
                AudioType audioType = GetAudioType(path);
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
                {
                    yield return request.SendWebRequest();

                    if (request.error == null)
                    {
                        audioClips[index] = DownloadHandlerAudioClip.GetContent(request);
                        audioClips[index].name = Path.GetFileNameWithoutExtension(soundPathsList[index]);
                    }
                }
            }

            index++;
        }
    }

    private AudioType GetAudioType(string path)
    {
        string extension = Path.GetExtension(path).ToLower();
        extension = extension.Replace(".", "");
        switch (extension)
        {
            case "ogg":
                return AudioType.OGGVORBIS;
            case "wav":
                return AudioType.WAV;
            case "aif":
            case "aiff":
                return AudioType.AIFF;
            case "mp3":
            case "m4a":
                return AudioType.MPEG;
            default:
                return AudioType.UNKNOWN;
        }
    }

    private void GenerateProfilesDisplay(string selectedValue)
    {
        if(Directory.Exists(ProfileData.FolderName))
        {
            string[] profiles = Directory.GetFiles(ProfileData.FolderName, "*" + ProfileData.FileExtension);
            List<string> namesList = new List<string>();
            for (int i = 0; i < profiles.Length; i++)
            {
                namesList.Add(Path.GetFileNameWithoutExtension(profiles[i]));
            }
            profilesDropdown.ClearOptions();
            profilesDropdown.AddOptions(namesList);
            profilesDropdown.value = profilesDropdown.options.FindIndex(option => option.text == selectedValue);
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
        button.colors = ThemeManager.currentColors;
        button.transform.SetParent(pomodorosScrollViewer.content.transform, false);
        button.GetComponent<Button>().onClick.AddListener(delegate { SelectPomodoro(index); });
        Text[] textBoxes = button.GetComponentsInChildren<Text>();
        textBoxes[0].text = ProfileData.Current.Pomodoros[index].Name;
        textBoxes[2].text = ProfileData.Current.Pomodoros[index].Duration.ToString();
        textBoxes[4].text = Path.GetFileNameWithoutExtension(ProfileData.Current.Pomodoros[index].SoundPath);
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
        int minutes = ProfileData.Current.Pomodoros[index].Duration / 60;
        int seconds = ProfileData.Current.Pomodoros[index].Duration % 60;
        pomMinutesInput.text = minutes.ToString();
        pomSecondsInput.text = seconds.ToString();
        int soundIndex = audioClips.FindIndex(c => c!= null && c.name == Path.GetFileNameWithoutExtension(ProfileData.Current.Pomodoros[index].SoundPath));
        soundsDropdown.value = soundIndex;
    }

    private void ClearPomodorosList()
    {
        foreach (Transform button in pomodorosScrollViewer.content)
        {
            Destroy(button.gameObject);
        }
    }

    public void AddPomodoroToList()
    {
        int totalDuration = GetTotalDuration();
        ProfileData.Current.Pomodoros.Add(new Pomodoro(pomNameInput.text, totalDuration, soundPathsList[soundsDropdown.value]));

        int pomodoroInd = ProfileData.Current.Pomodoros.Count - 1;
        CreatePomodoroPrefab(pomodoroInd);
        RefreshSelectionListeners();
    }

    public void EditPomodoro()
    {
        int selectedIndex = ProfileData.SelectedPomodoroIndex;
        if (selectedIndex != -1)
        {
            int totalDuration = GetTotalDuration();
            ProfileData.Current.Pomodoros[selectedIndex] = new Pomodoro(pomNameInput.text, totalDuration, soundPathsList[soundsDropdown.value]);
            UpdatePomodoroPrefabText(selectedIndex);
            if(selectedIndex == ProfileData.Current.PomodoroPlayIndex)
            {
                timeKeeper.LoadCurrentPomodoro();
                timeKeeper.ResetCurrentTime();
            }
        }
    }

    private int GetTotalDuration()
    {
        int minutes = 0;
        int.TryParse(pomMinutesInput.text, out minutes);
        int seconds = 0;
        int.TryParse(pomSecondsInput.text, out seconds);
        return Math.Abs(minutes) * 60 + Math.Abs(seconds);
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
        textBoxes[4].text = Path.GetFileNameWithoutExtension(ProfileData.Current.Pomodoros[index].SoundPath);
    }

    public void DisplayCurrentPomodoroName(string name)
    {
        PomodoroNameText.text = name;
    }

    public void SaveProfile()
    {
        if(profilesDropdown.options.Count > 0)
        {
            ProfileData.ChangeFileName(profilesDropdown.options[profilesDropdown.value].text);
            JsonIO.Save(ProfileData.Current);
            GenerateProfilesDisplay(ProfileData.FileName);
        }
    }

    public void AddProfile()
    {
        if (profileNameInput.text != "" && !DropdownContainsValue(profilesDropdown, profileNameInput.text))
        {
            ProfileData.ChangeFileName(profileNameInput.text);
            JsonIO.Save(ProfileData.Current);
            GenerateProfilesDisplay(ProfileData.FileName);
        }
    }

    public void DeleteProfile()
    {
        if(profilesDropdown.value >= 0 && profilesDropdown.value < profilesDropdown.options.Count)
        {
            if (profilesDropdown.options[profilesDropdown.value].text != ProfileData.DefaultFileName)
            {
                profilesDropdown.options.RemoveAt(profilesDropdown.value);
                profilesDropdown.value = 0;
                profilesDropdown.RefreshShownValue();
                ProfileData.ChangeFileName(profilesDropdown.options[profilesDropdown.value].text);
                File.Delete(ProfileData.SavePath);
            }
        }
    }

    private bool DropdownContainsValue(Dropdown dropdown,string value)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if(dropdown.options[i].text == value)
            {
                return true;
            }
        }
        return false;
    }

    public void LoadProfileFromName()
    {
        LoadProfile(profilesDropdown.options[profilesDropdown.value].text);
    }

    private void LoadProfile(string name)
    {
        ProfileData loadedData = (ProfileData)JsonIO.Load(name);
        if (loadedData != null)
        {
            ProfileData.Current = loadedData;
            ProfileData.ChangeFileName(name);
            volumeSlider.value = float.Parse(ProfileData.Current.Volume, CultureInfo.InvariantCulture.NumberFormat);
            profileNameText.text = String.Format("Profile: {0}", name);
            AddDefaultPomodoroFallback();
            timeKeeper.LoadCurrentPomodoro();
            themeManager.LoadProfileColors();

            GenerateProfilesDisplay(ProfileData.FileName);
            GeneratePomodorosDisplay();
            InitializeSoundsList();
            GenerateSoundsListFromPomodoros();
            backgroundImage.ChangeBackgroundImage(ProfileData.Current.BackgroundImagePath);
        }
    }

    private void LoadDefaultProfile()
    {
        // if there is no save data, set default values for app data
        ProfileData loadedData = (ProfileData)JsonIO.Load(ProfileData.DefaultFileName);
        if(loadedData != null)
        {
            ProfileData.Current = loadedData;
            
            AddDefaultPomodoroFallback();
        }
        else
        {
            ProfileData.Current.ButtonNormalColor = "#16A085";
            ProfileData.Current.ButtonHighlightedColor = "#1ABC9C";
            ProfileData.Current.Volume = "1";

            AddDefaultPomodoroFallback();

            JsonIO.Save(ProfileData.Current);
        }

        volumeSlider.value = float.Parse(ProfileData.Current.Volume, CultureInfo.InvariantCulture.NumberFormat);

        profileNameText.text = String.Format("Profile: {0}", ProfileData.DefaultFileName);
        timeKeeper.LoadCurrentPomodoro();
        themeManager.LoadProfileColors();
        
        GenerateProfilesDisplay(ProfileData.DefaultFileName);
        GeneratePomodorosDisplay();
        InitializeSoundsList();
        GenerateSoundsListFromPomodoros();
        backgroundImage.ChangeBackgroundImage(ProfileData.Current.BackgroundImagePath);
    }

    private void OnApplicationQuit()
    {
        // the data gets saved to the last filepath, so I need to reset it to default first before autosaving
        ProfileData.ChangeFileName(ProfileData.DefaultFileName);
        JsonIO.Save(ProfileData.Current);
    }
}
