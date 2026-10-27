using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// Handles the main menu, pause flow, audio settings, camera settings, and track selection.
public class UI : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterSlider, musicSlider, engineSlider, fovSlider;
    [SerializeField] Camera[] fovCameras;
    public GameObject MainUI, PlayUI, SettingUI, PauseUI, GameUI;
    public GameObject ZandvoortMap, NürburgringMap;
    GameObject trackInstance;
    public GameObject[] MenuUI;
    public bool paused, debugFoV, AIEnabled;
    float MasterValue = 1f, MusicValue = 1f, EngineValue = 1f, FoV = 60f;

    // Loads saved settings and initializes the UI state on startup.
    void Start()
    {
        Time.timeScale = 1f;
        paused = false;

        if (!PlayerPrefs.HasKey("AudioSettingsInitialized"))
        {
            PlayerPrefs.SetFloat("MasterAudio", 1f);
            PlayerPrefs.SetFloat("EngineAudio", 1f);
            PlayerPrefs.SetFloat("MusicAudio", 1f);
            PlayerPrefs.SetInt("AudioSettingsInitialized", 1);
            PlayerPrefs.Save();
        }

        MasterValue = PlayerPrefs.GetFloat("MasterAudio", 1f);
        EngineValue = PlayerPrefs.GetFloat("EngineAudio", 1f);
        MusicValue = PlayerPrefs.GetFloat("MusicAudio", 1f);
        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(MasterValue);
        }
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(MusicValue);
        }
        if (engineSlider != null)
        {
            engineSlider.SetValueWithoutNotify(EngineValue);
        }
        FoV = PlayerPrefs.GetFloat("CameraFoV", 60f);
        if (fovSlider != null)
        {
            fovSlider.onValueChanged.AddListener(value => CameraFoV(value));
            fovSlider.SetValueWithoutNotify(FoVSliderValue(FoV));
        }
        ApplyAudio("MasterAudio", MasterValue);
        ApplyAudio("EngineAudio", EngineValue);
        ApplyAudio("MusicAudio", MusicValue);
        ApplyFoV(FoV);
    }

    // Allows the player to pause or resume the game using Escape.
    void Update()
    {
        if (GameUI != null && (GameUI.activeSelf || paused))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    // Keeps the race UI hidden while the game is paused.
    void LateUpdate()
    {
        if (paused && GameUI != null)
        {
            GameUI.SetActive(false);
        }
    }

    // Toggle the pause menu and freeze or resume the game time.
    public void TogglePause()
    {
        if (SettingUI != null && SettingUI.activeSelf)
        {
            SettingUI.SetActive(false);
        }
        SetPaused(!paused);
    }

    // Applies the pause state to time scale and visible UI panels.
    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;

        if (PauseUI != null)
        {
            PauseUI.SetActive(paused);
        }
        if (GameUI != null)
        {
            GameUI.SetActive(!paused);
        }
    }

    // Restores normal time scale when the UI object is destroyed.
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // Opens the play selection screen from the main menu.
    public void PlayBtn()
    {
        MainUI.SetActive(false);
        PlayUI.SetActive(true);
    }

    // Opens the settings panel from either the main or pause menu.
    public void SettingBtn()
    {
        if (paused)
        {
            PauseUI.SetActive(false);
            SettingUI.SetActive(true);
        }
        else
        {
            MainUI.SetActive(false);
            SettingUI.SetActive(true);
        }
    }

    // Exits the application.
    public void ExitBtn()
    {
        Application.Quit();
    }

    // Returns the player to the menu and cleans up the active track instance.
    public void ToMenu()
    {
        SetPaused(false);
        GameUI.SetActive(false);
        PauseUI.SetActive(false);
        Destroy(trackInstance);
        for (int i = 0; i < MenuUI.Length - 1; i++)
        {
            MenuUI[i].SetActive(true);
        }
    }

    // Saves the AI toggle state to PlayerPrefs.
    public void ToggleAI(Toggle value)
    {
        AIEnabled = value.isOn;
        PlayerPrefs.SetInt("AIEnabled", AIEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Instantiates the Zandvoort course and hides the menu overlays.
    public void PlayZandvoort()
    {
        trackInstance = Instantiate(ZandvoortMap, new Vector3(0f, -0.12f, -50f), transform.rotation);
        for (int i = 0; i < MenuUI.Length; i++)
        {
            MenuUI[i].SetActive(false);
        }
        PlayUI.SetActive(false);
    }

    // Instantiates the Nürburgring course and hides the menu overlays.
    public void PlayNürburgring()
    {
        trackInstance = Instantiate(NürburgringMap, new Vector3(225f, 0.05f, -50f), transform.rotation);
        for (int i = 0; i < MenuUI.Length; i++)
        {
            MenuUI[i].SetActive(false);
        }
        PlayUI.SetActive(false);
    }

    // Returns to the previous menu screen based on the current state.
    public void BackBtn()
    {
        if (PlayUI.activeSelf)
        {
            PlayUI.SetActive(false);
            MainUI.SetActive(true);
        }
        else if (SettingUI.activeSelf)
        {
            SettingUI.SetActive(false);
            if (paused)
            {
                PauseUI.SetActive(true);
            }
            else
            {
                MainUI.SetActive(true);
            }
        }
    }

    // Applies the camera FOV value from the slider and updates all cameras.
    public void CameraFoV()
    {
        if (fovSlider != null)
        {
            FoV = FoVFromSlider(fovSlider.value);
        }
        ApplyFoV(FoV);
    }

    // Helper overload used by the slider's ValueChanged event.
    public void CameraFoV(float value)
    {
        FoV = fovSlider != null ? FoVFromSlider(value) : value;
        ApplyFoV(FoV);
    }

    // Converts the slider range to a camera FOV value in degrees.
    float FoVFromSlider(float value)
    {
        return fovSlider != null && fovSlider.maxValue <= 1f
            ? Mathf.Lerp(30f, 120f, value)
            : value;
    }

    // Converts the stored FOV back into the slider's normalized range.
    float FoVSliderValue(float value)
    {
        return fovSlider != null && fovSlider.maxValue <= 1f
            ? Mathf.InverseLerp(30f, 120f, value)
            : value;
    }

    // Applies the selected FOV to the scene cameras and stores it in PlayerPrefs.
    void ApplyFoV(float value)
    {
        FoV = Mathf.Clamp(value, 30f, 120f);
        PlayerPrefs.SetFloat("CameraFoV", FoV);
        PlayerPrefs.Save();

        Camera[] cameras = fovCameras;
        if (cameras != null)
        {
            foreach (Camera camera in fovCameras)
            {
                if (camera != null)
                {
                    camera.fieldOfView = FoV;
                }
            }
        }

        if (debugFoV)
        {
            Debug.Log($"FoV slider setting: {FoV:F1}; cameras: {FormatCameraFoV(cameras)}", this);
        }
    }

    // Formats the active camera list for debug output.
    string FormatCameraFoV(Camera[] cameras)
    {
        if (cameras == null || cameras.Length == 0)
        {
            return "none";
        }

        string result = "";
        foreach (Camera camera in cameras)
        {
            if (camera == null)
            {
                continue;
            }

            if (result.Length > 0)
            {
                result += ", ";
            }
            result += camera.name + "=" + camera.fieldOfView.ToString("F1");
        }
        return result.Length > 0 ? result : "none";
    }

    public void MasterAudio()
    {
        if (masterSlider != null)
        {
            MasterValue = masterSlider.value;
        }
        ApplyAudio("MasterAudio", MasterValue);
    }
    public void MasterAudio(float value)
    {
        MasterValue = value;
        MasterAudio();
    }
    public void MusicAudio()
    {
        if (musicSlider != null)
        {
            MusicValue = musicSlider.value;
        }
        ApplyAudio("MusicAudio", MusicValue);
    }
    public void MusicAudio(float value)
    {
        MusicValue = value;
        MusicAudio();
    }
    public void EngineAudio()
    {
        if (engineSlider != null)
        {
            EngineValue = engineSlider.value;
        }
        ApplyAudio("EngineAudio", EngineValue);
    }
    public void EngineAudio(float value)
    {
        EngineValue = value;
        EngineAudio();
    }

    void ApplyAudio(string parameter, float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp01(value) * 0.9999f + 0.0001f) * 20f;
        audioMixer.SetFloat(parameter, volume);
        PlayerPrefs.SetFloat(parameter, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

}
