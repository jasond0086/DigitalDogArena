using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    [Header("Pages")]
    public GameObject stablePage;
    public GameObject fightPage;
    public GameObject breedPage;
    public GameObject leaguePage;
    public GameObject storyPage;
    public GameObject settingsPage;

    [Header("Settings UI")]
    public Slider volumeSlider;
    public Toggle fightIntroToggle;
    public Toggle reducedEffectsToggle;
    public TMP_Dropdown textSpeedDropdown;
    public Button resetButton;
    public Button backButton;
    public TextMeshProUGUI settingsStatusText;

    GameObject currentPage;
    GameObject previousPage;

    void Start()
    {
        DigitalDogSettings.ApplyAudioSettings();
        ConfigureSettingsControls();
        LoadSettingsIntoUI();
        ShowStablePage();
    }

    public void ShowStablePage()
    {
        SetPage(stablePage);
    }

    public void ShowFightPage()
    {
        SetPage(fightPage);
    }

    public void ShowBreedPage()
    {
        SetPage(breedPage);
    }

    public void ShowLeaguePage()
    {
        SetPage(leaguePage);
    }

    public void ShowStoryPage()
    {
        SetPage(storyPage);
    }

    public void ShowSettingsPage()
    {
        previousPage = currentPage != settingsPage ? currentPage : previousPage;

        if (settingsPage == null)
        {
            Debug.LogWarning("PageManager.ShowSettingsPage was called, but settingsPage is not assigned.");
            return;
        }

        LoadSettingsIntoUI();
        SetPage(settingsPage);
    }

    public void HideSettingsPage()
    {
        BackFromSettings();
    }

    public void BackFromSettings()
    {
        ShowPreviousPage();
    }

    public void ShowPreviousPage()
    {
        if (previousPage != null)
        {
            SetPage(previousPage);
            return;
        }

        ShowStablePage();
    }

    void SetPage(GameObject pageToShow)
    {
        if (stablePage != null)
        {
            stablePage.SetActive(pageToShow == stablePage);
        }

        if (fightPage != null)
        {
            fightPage.SetActive(pageToShow == fightPage);
        }

        if (breedPage != null)
        {
            breedPage.SetActive(pageToShow == breedPage);
        }

        if (leaguePage != null)
        {
            leaguePage.SetActive(pageToShow == leaguePage);
        }

        if (storyPage != null)
        {
            storyPage.SetActive(pageToShow == storyPage);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(pageToShow == settingsPage);
        }

        if (pageToShow != null)
        {
            currentPage = pageToShow;
        }
    }

    void ConfigureSettingsControls()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        else
        {
            Debug.LogWarning("PageManager volumeSlider is not assigned. Master Volume can still be set by calling SetMasterVolume.");
        }

        if (fightIntroToggle != null)
        {
            fightIntroToggle.onValueChanged.RemoveListener(SetFightIntroEnabled);
            fightIntroToggle.onValueChanged.AddListener(SetFightIntroEnabled);
        }
        else
        {
            Debug.LogWarning("PageManager fightIntroToggle is not assigned.");
        }

        if (reducedEffectsToggle != null)
        {
            reducedEffectsToggle.onValueChanged.RemoveListener(SetReducedEffectsEnabled);
            reducedEffectsToggle.onValueChanged.AddListener(SetReducedEffectsEnabled);
        }
        else
        {
            Debug.LogWarning("PageManager reducedEffectsToggle is not assigned.");
        }

        if (textSpeedDropdown != null)
        {
            textSpeedDropdown.onValueChanged.RemoveListener(SetTextSpeed);
            textSpeedDropdown.onValueChanged.AddListener(SetTextSpeed);
        }
        else
        {
            Debug.LogWarning("PageManager textSpeedDropdown is not assigned. Text Speed can still be set by calling SetTextSpeedSlow/Normal/Fast.");
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetSettingsToDefaults);
            resetButton.onClick.AddListener(ResetSettingsToDefaults);
        }
        else
        {
            Debug.LogWarning("PageManager resetButton is not assigned.");
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(BackFromSettings);
            backButton.onClick.AddListener(BackFromSettings);
        }
        else
        {
            Debug.LogWarning("PageManager backButton is not assigned.");
        }
    }

    public void LoadSettingsIntoUI()
    {
        DigitalDogSettings.ApplyAudioSettings();

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(DigitalDogSettings.MasterVolume);
        }

        if (fightIntroToggle != null)
        {
            fightIntroToggle.SetIsOnWithoutNotify(DigitalDogSettings.FightIntroEnabled);
        }

        if (reducedEffectsToggle != null)
        {
            reducedEffectsToggle.SetIsOnWithoutNotify(DigitalDogSettings.ReducedEffectsEnabled);
        }

        if (textSpeedDropdown != null)
        {
            EnsureTextSpeedDropdownOptions();
            textSpeedDropdown.SetValueWithoutNotify((int)DigitalDogSettings.CurrentTextSpeed);
            textSpeedDropdown.RefreshShownValue();
        }
    }

    void EnsureTextSpeedDropdownOptions()
    {
        if (textSpeedDropdown == null)
        {
            return;
        }

        if (textSpeedDropdown.options.Count >= 3)
        {
            return;
        }

        textSpeedDropdown.options.Clear();
        textSpeedDropdown.options.Add(new TMP_Dropdown.OptionData("Slow"));
        textSpeedDropdown.options.Add(new TMP_Dropdown.OptionData("Normal"));
        textSpeedDropdown.options.Add(new TMP_Dropdown.OptionData("Fast"));
    }

    public void SetMasterVolume(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        DigitalDogSettings.MasterVolume = clampedValue;

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(DigitalDogSettings.MasterVolume);
        }

        SetSettingsStatus($"Master volume: {Mathf.RoundToInt(DigitalDogSettings.MasterVolume * 100f)}%");
    }

    public void SetFightIntroEnabled(bool enabled)
    {
        DigitalDogSettings.FightIntroEnabled = enabled;
        SetSettingsStatus(enabled ? "Fight intro enabled." : "Fight intro disabled.");
    }

    public void SetReducedEffectsEnabled(bool enabled)
    {
        DigitalDogSettings.ReducedEffectsEnabled = enabled;
        SetSettingsStatus(enabled ? "Reduced effects enabled." : "Full effects enabled.");
    }

    public void SetTextSpeed(int option)
    {
        int safeOption = Mathf.Clamp(option, 0, 2);
        DigitalDogSettings.CurrentTextSpeed = (DigitalDogSettings.TextSpeed)safeOption;
        SetSettingsStatus($"Text speed: {DigitalDogSettings.CurrentTextSpeed}");
    }

    public void SetTextSpeedSlow()
    {
        SetTextSpeed((int)DigitalDogSettings.TextSpeed.Slow);
        LoadSettingsIntoUI();
    }

    public void SetTextSpeedNormal()
    {
        SetTextSpeed((int)DigitalDogSettings.TextSpeed.Normal);
        LoadSettingsIntoUI();
    }

    public void SetTextSpeedFast()
    {
        SetTextSpeed((int)DigitalDogSettings.TextSpeed.Fast);
        LoadSettingsIntoUI();
    }

    public void ResetSettingsToDefaults()
    {
        DigitalDogSettings.ResetToDefaults();
        LoadSettingsIntoUI();
        SetSettingsStatus("Settings reset to defaults.");
    }

    void SetSettingsStatus(string message)
    {
        if (settingsStatusText != null)
        {
            settingsStatusText.text = message;
        }
    }
}

public static class DigitalDogSettings
{
    public enum TextSpeed
    {
        Slow = 0,
        Normal = 1,
        Fast = 2
    }

    public const float DefaultMasterVolume = 1f;
    public const bool DefaultFightIntroEnabled = true;
    public const bool DefaultReducedEffectsEnabled = false;
    public const TextSpeed DefaultTextSpeed = TextSpeed.Normal;

    const string MasterVolumeKey = "DigitalDogArena.Settings.MasterVolume";
    const string FightIntroKey = "DigitalDogArena.Settings.FightIntroEnabled";
    const string ReducedEffectsKey = "DigitalDogArena.Settings.ReducedEffectsEnabled";
    const string TextSpeedKey = "DigitalDogArena.Settings.TextSpeed";

    public static float MasterVolume
    {
        get
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
        }
        set
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
            ApplyAudioSettings();
            PlayerPrefs.Save();
        }
    }

    public static bool FightIntroEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(FightIntroKey, DefaultFightIntroEnabled ? 1 : 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(FightIntroKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool ReducedEffectsEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(ReducedEffectsKey, DefaultReducedEffectsEnabled ? 1 : 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(ReducedEffectsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static TextSpeed CurrentTextSpeed
    {
        get
        {
            int savedValue = PlayerPrefs.GetInt(TextSpeedKey, (int)DefaultTextSpeed);

            if (savedValue < (int)TextSpeed.Slow || savedValue > (int)TextSpeed.Fast)
            {
                return DefaultTextSpeed;
            }

            return (TextSpeed)savedValue;
        }
        set
        {
            PlayerPrefs.SetInt(TextSpeedKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static void ApplyAudioSettings()
    {
        AudioListener.volume = MasterVolume;
    }

    public static void ResetToDefaults()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, DefaultMasterVolume);
        PlayerPrefs.SetInt(FightIntroKey, DefaultFightIntroEnabled ? 1 : 0);
        PlayerPrefs.SetInt(ReducedEffectsKey, DefaultReducedEffectsEnabled ? 1 : 0);
        PlayerPrefs.SetInt(TextSpeedKey, (int)DefaultTextSpeed);
        ApplyAudioSettings();
        PlayerPrefs.Save();
    }
}
