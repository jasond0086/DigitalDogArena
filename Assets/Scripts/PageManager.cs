using System;
using System.Collections.Generic;
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
    public StoryManager storyManager;
    public FightManager fightManager;

    [Header("Settings UI")]
    public Slider volumeSlider;
    public Toggle fightIntroToggle;
    public Toggle reducedEffectsToggle;
    public TMP_Dropdown textSpeedDropdown;
    public Button resetButton;
    public Button backButton;
    public TextMeshProUGUI settingsStatusText;

    [Header("Training Page")]
    public GameObject trainingPage;
    public Button trainingButton;
    public TMP_Dropdown trainingDogDropdown;
    public Image trainingDogImage;
    public TextMeshProUGUI trainingDogNameText;
    public TextMeshProUGUI trainingDogBreedText;
    public TextMeshProUGUI trainingDogStatsText;
    public TextMeshProUGUI trainingStatusText;
    public Button powerDrillButton;
    public Button sprintDrillButton;
    public Button enduranceDrillButton;
    public Button focusDrillButton;
    public Button trainingBackButton;

    [Header("Dog Pound Page")]
    public GameObject dogPoundPage;
    public Button dogPoundButton;
    public DogManager dogManager;
    public TextMeshProUGUI dogPoundTokenText;
    public TextMeshProUGUI dogPoundTimerText;
    public TextMeshProUGUI dogPoundStatusText;
    public GameObject[] poundDogSlots = new GameObject[3];
    public TextMeshProUGUI[] poundDogNameTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogBreedTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogSexTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogStatsTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogStrategyTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogTraitsTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] poundDogCostTexts = new TextMeshProUGUI[3];
    public Button[] poundDogSelectButtons = new Button[3];
    public Button adoptButton;
    public Button refreshPoundButton;
    public Button buyTokensButton;
    public Button dogPoundBackButton;
    public GameObject narratorPanel;

    GameObject currentPage;
    GameObject previousPage;

    const int DogPoundSlotCount = 3;
    static readonly TimeSpan DogPoundRefreshPeriod = TimeSpan.FromHours(24);

    DogPoundInventoryData dogPoundInventory;
    DateTime dogPoundLastRefreshUtc;
    int selectedPoundDogSlot = -1;
    bool narratorWasVisibleBeforeDogPound;
    readonly List<TextMeshProUGUI> dogPoundStatusTexts = new List<TextMeshProUGUI>();
    readonly Color[] poundDogSlotBaseColors = new Color[DogPoundSlotCount];
    readonly bool[] poundDogSlotBaseColorsCaptured = new bool[DogPoundSlotCount];
    readonly List<Dog> trainingDogs = new List<Dog>();
    int selectedTrainingDogIndex = -1;
    bool narratorWasVisibleBeforeTraining;
    bool reportedMissingTrainingSceneReferences;

    enum TrainingDrill
    {
        Power,
        Sprint,
        Endurance,
        Focus
    }

    void Start()
    {
        DigitalDogSettings.ApplyAudioSettings();
        ConfigureSettingsControls();
        LoadSettingsIntoUI();
        BindTrainingSceneReferencesIfMissing();
        ConfigureTrainingControls();
        BindDogPoundSceneReferencesIfMissing();
        ConfigureDogPoundControls();
        ShowStablePage();
    }

    public void ShowStablePage()
    {
        SetPage(stablePage);
    }

    public void ShowFightPage()
    {
        SetPage(fightPage);

        if (fightManager == null)
        {
            fightManager = GetComponent<FightManager>();
        }

        if (fightManager != null)
        {
            fightManager.RefreshFightPreviews(true);
        }
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
        previousPage = currentPage != storyPage ? currentPage : previousPage;
        SetPage(storyPage);

        if (storyManager == null)
        {
            storyManager = GetComponent<StoryManager>();
        }

        if (storyManager != null)
        {
            storyManager.RefreshStoryUI();
        }
    }

    public void BackFromStoryPage()
    {
        ShowPreviousPage();
    }

    public void ResetStoryProgress()
    {
        if (storyManager == null)
        {
            storyManager = GetComponent<StoryManager>();
        }

        if (storyManager == null)
        {
            Debug.LogWarning("PageManager.ResetStoryProgress could not find StoryManager.");
            return;
        }

        storyManager.ResetStoryProgress();
    }

    public void DebugAddKennelReputation()
    {
        if (storyManager == null)
        {
            storyManager = GetComponent<StoryManager>();
        }

        if (storyManager == null)
        {
            Debug.LogWarning("PageManager.DebugAddKennelReputation could not find StoryManager.");
            return;
        }

        storyManager.AddKennelReputation(25);
        Debug.Log($"Debug Kennel Reputation added. Total: {storyManager.GetKennelReputation()}.");
    }

    public void EndCurrentFight()
    {
        if (fightManager == null)
        {
            fightManager = GetComponent<FightManager>();
        }

        if (fightManager == null)
        {
            Debug.LogWarning("PageManager.EndCurrentFight could not find FightManager.");
            return;
        }

        fightManager.EndCurrentFight();
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

    public void ShowDogPoundPage()
    {
        BindDogPoundSceneReferencesIfMissing();
        ConfigureDogPoundControls();

        if (dogPoundPage == null)
        {
            Debug.LogWarning("PageManager.ShowDogPoundPage was called, but DogPoundPage was not found or assigned.");
            return;
        }

        previousPage = currentPage != dogPoundPage ? currentPage : previousPage;

        if (!EnsureDogManager())
        {
            SetDogPoundStatus("Dog Pound is unavailable because DogManager is missing.");
            return;
        }

        LoadOrCreateDogPoundInventory();
        selectedPoundDogSlot = -1;
        HideNarratorForDogPound();
        RefreshDogPoundUI();
        SetPage(dogPoundPage);
    }

    public void ShowTrainingPage()
    {
        BindTrainingSceneReferencesIfMissing();
        ConfigureTrainingControls();

        if (trainingPage == null)
        {
            Debug.LogWarning("PageManager.ShowTrainingPage was called, but TrainingPage is not assigned or found.");
            return;
        }

        ReportMissingTrainingSceneReferences();
        previousPage = currentPage != trainingPage ? currentPage : previousPage;
        HideNarratorForTraining();
        SetPage(trainingPage);
        PopulateTrainingDogDropdown();
    }

    public void BackFromTraining()
    {
        selectedTrainingDogIndex = -1;
        trainingDogs.Clear();
        RestoreNarratorAfterTraining();
        ShowPreviousPage();
    }

    public void SelectTrainingDog(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= trainingDogs.Count)
        {
            selectedTrainingDogIndex = -1;
            RefreshTrainingDogPreview();
            SetTrainingStatus("Select a dog first.");
            return;
        }

        selectedTrainingDogIndex = optionIndex;
        RefreshTrainingDogPreview();

        Dog selectedDog = GetSelectedTrainingDog();
        SetTrainingStatus(selectedDog != null
            ? $"Choose a drill for {GetTrainingDogName(selectedDog)}."
            : "Select a dog first.");
    }

    public void TrainStrength()
    {
        TrainSelectedDog(TrainingDrill.Power, "Power Drill", "STR");
    }

    public void TrainAgility()
    {
        TrainSelectedDog(TrainingDrill.Sprint, "Sprint Drill", "AGI");
    }

    public void TrainStamina()
    {
        TrainSelectedDog(TrainingDrill.Endurance, "Endurance Drill", "STA");
    }

    public void TrainIntelligence()
    {
        TrainSelectedDog(TrainingDrill.Focus, "Focus Drill", "INT");
    }

    public void BackFromDogPound()
    {
        selectedPoundDogSlot = -1;
        RestoreNarratorAfterDogPound();
        ShowPreviousPage();
    }

    public void SelectPoundDogSlot0()
    {
        SelectPoundDogSlot(0);
    }

    public void SelectPoundDogSlot1()
    {
        SelectPoundDogSlot(1);
    }

    public void SelectPoundDogSlot2()
    {
        SelectPoundDogSlot(2);
    }

    public void AdoptSelectedDog()
    {
        Debug.Log($"Dog Pound adoption requested. Selected slot: {selectedPoundDogSlot}. Tokens: {DogPoundSettings.TokenCount}.");

        if (!EnsureDogManager())
        {
            ReportDogPoundAdoptionFailure("Dog Pound is unavailable because DogManager is missing.");
            return;
        }

        LoadOrCreateDogPoundInventory();

        if (selectedPoundDogSlot < 0)
        {
            RefreshDogPoundUI();
            ReportDogPoundAdoptionFailure("Select a dog first.");
            return;
        }

        DogPoundOfferData offer = GetPoundOffer(selectedPoundDogSlot);

        if (offer == null || offer.dog == null)
        {
            RefreshDogPoundUI();
            ReportDogPoundAdoptionFailure("Selected dog data is missing.");
            return;
        }

        if (offer.adopted)
        {
            RefreshDogPoundUI();
            ReportDogPoundAdoptionFailure("Selected dog is no longer available.");
            return;
        }

        Debug.Log($"Dog Pound selected dog cost: {offer.tokenCost}. Token count: {DogPoundSettings.TokenCount}.");

        if (DogPoundSettings.TokenCount < offer.tokenCost)
        {
            int tokensNeeded = offer.tokenCost - DogPoundSettings.TokenCount;
            RefreshDogPoundUI();
            ReportDogPoundAdoptionFailure($"Not enough Dog Pound Tokens. Need {tokensNeeded} more.");
            return;
        }

        Dog adoptedDog = dogManager.CreateDogFromSaveData(offer.dog);

        if (adoptedDog == null)
        {
            RefreshDogPoundUI();
            ReportDogPoundAdoptionFailure("Selected dog data is missing.");
            return;
        }

        DogPoundSettings.TokenCount -= offer.tokenCost;
        dogManager.AddOwnedDog(adoptedDog);
        offer.adopted = true;
        SaveDogPoundInventory();

        string adoptedName = string.IsNullOrWhiteSpace(adoptedDog.dogName) ? "Dog" : adoptedDog.dogName;
        selectedPoundDogSlot = -1;
        RefreshDogPoundUI();
        SetDogPoundStatus($"Adopted {adoptedName}.");
        Debug.Log($"Dog Pound adoption succeeded. Adopted {adoptedName}. Tokens remaining: {DogPoundSettings.TokenCount}.");
    }

    public void RefreshDogPound()
    {
        if (!EnsureDogManager())
        {
            SetDogPoundStatus("Dog Pound is unavailable because DogManager is missing.");
            return;
        }

        if (dogPoundInventory == null || !IsDogPoundInventoryValid(dogPoundInventory))
        {
            if (!DogPoundSettings.TryLoadInventory(out dogPoundInventory, out dogPoundLastRefreshUtc) ||
                !IsDogPoundInventoryValid(dogPoundInventory))
            {
                GenerateNewDogPoundInventory();
                selectedPoundDogSlot = -1;
                SetDogPoundStatus("Three new pound dogs are available.");
                RefreshDogPoundUI();
                return;
            }
        }

        if (!IsDogPoundRefreshReady())
        {
            SetDogPoundStatus($"New pound dogs available in {GetDogPoundRefreshRemainingText()}.");
            RefreshDogPoundUI();
            return;
        }

        GenerateNewDogPoundInventory();
        selectedPoundDogSlot = -1;
        SetDogPoundStatus("Three new pound dogs are available.");
        RefreshDogPoundUI();
    }

    public void BuyDogPoundTokensPlaceholder()
    {
        SetDogPoundStatus("Buy Tokens is not implemented in this build.");
    }

    public void DebugAddDogPoundTokens()
    {
        DogPoundSettings.AddTokens(20);
        Debug.Log($"Dog Pound debug tokens added. Token count: {DogPoundSettings.TokenCount}.");

        if (dogPoundInventory == null && EnsureDogManager())
        {
            LoadOrCreateDogPoundInventory();
        }

        RefreshDogPoundUI();
        SetDogPoundStatus("Added 20 Dog Pound Tokens.");
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

        if (trainingPage != null)
        {
            bool showingTraining = pageToShow == trainingPage;
            trainingPage.SetActive(showingTraining);

            if (!showingTraining)
            {
                RestoreNarratorAfterTraining();
            }
        }

        if (dogPoundPage != null)
        {
            bool showingDogPound = pageToShow == dogPoundPage;
            dogPoundPage.SetActive(showingDogPound);

            if (!showingDogPound)
            {
                RestoreNarratorAfterDogPound();
            }
        }

        if (pageToShow != null)
        {
            currentPage = pageToShow;
        }
    }

    void Update()
    {
        if (dogPoundPage != null && dogPoundPage.activeInHierarchy && dogPoundInventory != null)
        {
            UpdateDogPoundTimerText();
        }
    }

    void BindTrainingSceneReferencesIfMissing()
    {
        if (trainingPage == null)
        {
            trainingPage = FindSceneObjectByTrimmedName("TrainingPage");
        }

        if (trainingButton == null)
        {
            GameObject buttonObject = FindSceneObjectByTrimmedName("TrainingButton") ??
                                      FindSceneObjectByTrimmedName("TrainingTabButton");
            trainingButton = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        }

        if (narratorPanel == null)
        {
            narratorPanel = FindSceneObjectByTrimmedName("NarratorPanel");
        }

        if (trainingPage == null)
        {
            return;
        }

        Transform pageRoot = trainingPage.transform;
        trainingDogDropdown = trainingDogDropdown ??
                              (FindGameObjectInTree(pageRoot, "DogDropdown") ??
                               FindGameObjectInTree(pageRoot, "DogDropDown"))?.GetComponent<TMP_Dropdown>();
        trainingDogImage = trainingDogImage ?? FindGameObjectInTree(pageRoot, "DogImage")?.GetComponent<Image>();
        trainingDogNameText = trainingDogNameText ?? FindTextInTree(pageRoot, "DogNameText");
        trainingDogBreedText = trainingDogBreedText ?? FindTextInTree(pageRoot, "DogBreedText");
        trainingDogStatsText = trainingDogStatsText ?? FindTextInTree(pageRoot, "DogStatsText");
        trainingStatusText = trainingStatusText ?? FindTextInTree(pageRoot, "TrainingStatusText");
        powerDrillButton = powerDrillButton ?? FindButtonInTree(pageRoot, "PowerDrillButton");
        sprintDrillButton = sprintDrillButton ?? FindButtonInTree(pageRoot, "SprintDrillButton");
        enduranceDrillButton = enduranceDrillButton ?? FindButtonInTree(pageRoot, "EnduranceDrillButton");
        focusDrillButton = focusDrillButton ?? FindButtonInTree(pageRoot, "FocusDrillButton");
        trainingBackButton = trainingBackButton ?? FindButtonInTree(pageRoot, "BackButton");
    }

    void ConfigureTrainingControls()
    {
        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveListener(ShowTrainingPage);
            trainingButton.onClick.AddListener(ShowTrainingPage);
        }

        if (trainingDogDropdown != null)
        {
            trainingDogDropdown.onValueChanged.RemoveListener(SelectTrainingDog);
            trainingDogDropdown.onValueChanged.AddListener(SelectTrainingDog);
        }

        ConfigureTrainingButton(powerDrillButton, TrainStrength);
        ConfigureTrainingButton(sprintDrillButton, TrainAgility);
        ConfigureTrainingButton(enduranceDrillButton, TrainStamina);
        ConfigureTrainingButton(focusDrillButton, TrainIntelligence);
        ConfigureTrainingButton(trainingBackButton, BackFromTraining);
    }

    void ConfigureTrainingButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    void PopulateTrainingDogDropdown()
    {
        trainingDogs.Clear();
        selectedTrainingDogIndex = -1;

        if (!EnsureDogManager())
        {
            ClearTrainingDogDropdown();
            RefreshTrainingDogPreview();
            SetTrainingStatus("No dogs available to train.");
            return;
        }

        foreach (Dog dog in dogManager.ownedDogs)
        {
            if (dog != null)
            {
                trainingDogs.Add(dog);
            }
        }

        if (trainingDogDropdown != null)
        {
            trainingDogDropdown.ClearOptions();

            List<string> dogNames = new List<string>();
            foreach (Dog dog in trainingDogs)
            {
                dogNames.Add(GetTrainingDogName(dog));
            }

            trainingDogDropdown.AddOptions(dogNames);
            trainingDogDropdown.interactable = trainingDogs.Count > 0;
        }

        if (trainingDogs.Count == 0)
        {
            RefreshTrainingDogPreview();
            SetTrainingStatus("No dogs available to train.");
            return;
        }

        selectedTrainingDogIndex = 0;

        if (trainingDogDropdown != null)
        {
            trainingDogDropdown.SetValueWithoutNotify(selectedTrainingDogIndex);
            trainingDogDropdown.RefreshShownValue();
        }

        RefreshTrainingDogPreview();
        SetTrainingStatus("Choose a dog to train.");
    }

    void ClearTrainingDogDropdown()
    {
        if (trainingDogDropdown == null)
        {
            return;
        }

        trainingDogDropdown.ClearOptions();
        trainingDogDropdown.interactable = false;
        trainingDogDropdown.RefreshShownValue();
    }

    void RefreshTrainingDogPreview()
    {
        Dog selectedDog = GetSelectedTrainingDog();
        bool hasSelectedDog = selectedDog != null;

        SetTrainingDrillButtonsInteractable(hasSelectedDog);

        if (!hasSelectedDog)
        {
            SetTrainingPreviewText("No dog selected", "Breed: --", "STR -- / AGI --\nSTA -- / INT --");
            SetTrainingDogImage(null);
            return;
        }

        SetTrainingPreviewText(
            GetTrainingDogName(selectedDog),
            string.IsNullOrWhiteSpace(selectedDog.breed) ? "Unknown breed" : selectedDog.breed.Trim(),
            $"STR {selectedDog.strength} / AGI {selectedDog.agility}\nSTA {selectedDog.stamina} / INT {selectedDog.GetIntelligence()}");
        SetTrainingDogImage(selectedDog);
    }

    void SetTrainingPreviewText(string dogName, string breed, string stats)
    {
        if (trainingDogNameText != null)
        {
            trainingDogNameText.text = dogName;
        }

        if (trainingDogBreedText != null)
        {
            trainingDogBreedText.text = breed;
        }

        if (trainingDogStatsText != null)
        {
            trainingDogStatsText.text = stats;
        }
    }

    void SetTrainingDogImage(Dog dog)
    {
        if (trainingDogImage == null)
        {
            return;
        }

        Sprite portrait = dog != null
            ? DogPortraitLibrary.ChooseStableCardPortrait(dog, dog.dogSprite, null)
            : null;

        trainingDogImage.sprite = portrait;
        trainingDogImage.enabled = portrait != null;
    }

    void SetTrainingDrillButtonsInteractable(bool isInteractable)
    {
        if (powerDrillButton != null)
        {
            powerDrillButton.interactable = isInteractable;
        }

        if (sprintDrillButton != null)
        {
            sprintDrillButton.interactable = isInteractable;
        }

        if (enduranceDrillButton != null)
        {
            enduranceDrillButton.interactable = isInteractable;
        }

        if (focusDrillButton != null)
        {
            focusDrillButton.interactable = isInteractable;
        }
    }

    Dog GetSelectedTrainingDog()
    {
        if (selectedTrainingDogIndex < 0 || selectedTrainingDogIndex >= trainingDogs.Count)
        {
            return null;
        }

        return trainingDogs[selectedTrainingDogIndex];
    }

    void TrainSelectedDog(TrainingDrill drill, string drillName, string statLabel)
    {
        if (!EnsureDogManager())
        {
            SetTrainingStatus("No dogs available to train.");
            return;
        }

        Dog selectedDog = GetSelectedTrainingDog();

        if (selectedDog == null || !dogManager.ownedDogs.Contains(selectedDog))
        {
            SetTrainingStatus("Select a dog first.");
            return;
        }

        int updatedStat;

        switch (drill)
        {
            case TrainingDrill.Power:
                selectedDog.strength += 1;
                updatedStat = selectedDog.strength;
                break;

            case TrainingDrill.Sprint:
                selectedDog.agility += 1;
                updatedStat = selectedDog.agility;
                break;

            case TrainingDrill.Endurance:
                selectedDog.stamina += 1;
                updatedStat = selectedDog.stamina;
                break;

            default:
                selectedDog.intelligence += 1;
                updatedStat = selectedDog.GetIntelligence();
                break;
        }

        dogManager.SaveStable();
        dogManager.DisplayDogs();
        RefreshTrainingDogPreview();
        SetTrainingStatus($"{GetTrainingDogName(selectedDog)} completed {drillName}. {statLabel} increased to {updatedStat}.");
        Debug.Log($"Training complete: {GetTrainingDogName(selectedDog)} completed {drillName}. {statLabel}: {updatedStat}.");
    }

    string GetTrainingDogName(Dog dog)
    {
        return dog == null || string.IsNullOrWhiteSpace(dog.dogName) ? "Dog" : dog.dogName.Trim();
    }

    void SetTrainingStatus(string message)
    {
        if (trainingStatusText != null)
        {
            trainingStatusText.text = message;
        }
    }

    void ReportMissingTrainingSceneReferences()
    {
        if (reportedMissingTrainingSceneReferences)
        {
            return;
        }

        List<string> missingReferences = new List<string>();

        if (trainingDogDropdown == null) missingReferences.Add("DogDropdown");
        if (trainingDogImage == null) missingReferences.Add("DogImage");
        if (trainingDogNameText == null) missingReferences.Add("DogNameText");
        if (trainingDogBreedText == null) missingReferences.Add("DogBreedText");
        if (trainingDogStatsText == null) missingReferences.Add("DogStatsText");
        if (trainingStatusText == null) missingReferences.Add("TrainingStatusText");
        if (powerDrillButton == null) missingReferences.Add("PowerDrillButton");
        if (sprintDrillButton == null) missingReferences.Add("SprintDrillButton");
        if (enduranceDrillButton == null) missingReferences.Add("EnduranceDrillButton");
        if (focusDrillButton == null) missingReferences.Add("FocusDrillButton");
        if (trainingBackButton == null) missingReferences.Add("BackButton");

        if (missingReferences.Count > 0)
        {
            Debug.LogWarning($"TrainingPage is missing scene references: {string.Join(", ", missingReferences)}.");
        }

        reportedMissingTrainingSceneReferences = true;
    }

    void HideNarratorForTraining()
    {
        if (narratorPanel == null)
        {
            narratorPanel = FindSceneObjectByTrimmedName("NarratorPanel");
        }

        if (narratorPanel == null)
        {
            return;
        }

        narratorWasVisibleBeforeTraining = narratorPanel.activeSelf;
        narratorPanel.SetActive(false);
    }

    void RestoreNarratorAfterTraining()
    {
        if (narratorPanel != null && narratorWasVisibleBeforeTraining)
        {
            narratorPanel.SetActive(true);
        }

        narratorWasVisibleBeforeTraining = false;
    }

    void BindDogPoundSceneReferencesIfMissing()
    {
        if (dogPoundPage == null)
        {
            dogPoundPage = FindSceneObjectByTrimmedName("DogPoundPage");
        }

        if (dogPoundButton == null)
        {
            GameObject buttonObject = FindSceneObjectByTrimmedName("DogPoundButton");
            dogPoundButton = buttonObject != null ? buttonObject.GetComponent<Button>() : null;
        }

        if (narratorPanel == null)
        {
            narratorPanel = FindSceneObjectByTrimmedName("NarratorPanel");
        }

        if (dogPoundPage == null)
        {
            return;
        }

        Transform pageRoot = dogPoundPage.transform;
        dogPoundTokenText = dogPoundTokenText ?? FindTextInTree(pageRoot, "DogPoundTokenText");
        dogPoundTimerText = dogPoundTimerText ?? FindTextInTree(pageRoot, "DogPoundTimerText");
        BindDogPoundStatusTexts(pageRoot);
        adoptButton = adoptButton ?? FindButtonInTree(pageRoot, "AdoptButton");
        refreshPoundButton = refreshPoundButton ?? FindButtonInTree(pageRoot, "RefreshPoundButton", "RefreshButton");
        buyTokensButton = buyTokensButton ?? FindButtonInTree(pageRoot, "BuyTokensButton");
        dogPoundBackButton = dogPoundBackButton ?? FindButtonInTree(pageRoot, "BackButton");

        for (int slotIndex = 0; slotIndex < DogPoundSlotCount; slotIndex++)
        {
            if (poundDogSlots[slotIndex] == null)
            {
                poundDogSlots[slotIndex] = FindGameObjectInTree(pageRoot, $"PoundDogSlot{slotIndex}");
            }

            if (poundDogSlots[slotIndex] == null)
            {
                continue;
            }

            Transform slotRoot = poundDogSlots[slotIndex].transform;
            poundDogNameTexts[slotIndex] = poundDogNameTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogNameText");
            poundDogBreedTexts[slotIndex] = poundDogBreedTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogBreedText");
            poundDogSexTexts[slotIndex] = poundDogSexTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogSexText");
            poundDogStatsTexts[slotIndex] = poundDogStatsTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogStatsText");
            poundDogStrategyTexts[slotIndex] = poundDogStrategyTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogStrategyText");
            poundDogTraitsTexts[slotIndex] = poundDogTraitsTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogTraitsText");
            poundDogCostTexts[slotIndex] = poundDogCostTexts[slotIndex] ?? FindTextInTree(slotRoot, "DogCostText");
            poundDogSelectButtons[slotIndex] = poundDogSelectButtons[slotIndex] ?? slotRoot.GetComponentInChildren<Button>(true);
        }
    }

    void ConfigureDogPoundControls()
    {
        if (dogPoundButton != null)
        {
            dogPoundButton.onClick.RemoveListener(ShowDogPoundPage);
            dogPoundButton.onClick.AddListener(ShowDogPoundPage);
        }

        ConfigurePoundSlotButton(0, SelectPoundDogSlot0);
        ConfigurePoundSlotButton(1, SelectPoundDogSlot1);
        ConfigurePoundSlotButton(2, SelectPoundDogSlot2);

        ConfigurePoundButton(adoptButton, AdoptSelectedDog);
        ConfigurePoundButton(refreshPoundButton, RefreshDogPound);
        ConfigurePoundButton(buyTokensButton, BuyDogPoundTokensPlaceholder);
        ConfigurePoundButton(dogPoundBackButton, BackFromDogPound);
    }

    void BindDogPoundStatusTexts(Transform pageRoot)
    {
        dogPoundStatusTexts.Clear();

        foreach (TextMeshProUGUI statusText in pageRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (string.Equals(statusText.name.Trim(), "DogPoundStatusText", StringComparison.Ordinal))
            {
                dogPoundStatusTexts.Add(statusText);
            }
        }

        if (dogPoundStatusTexts.Count == 0)
        {
            if (dogPoundStatusText != null)
            {
                dogPoundStatusTexts.Add(dogPoundStatusText);
            }
            else
            {
                Debug.LogWarning("Dog Pound Status Text could not be found under DogPoundPage.");
            }
        }

        foreach (TextMeshProUGUI statusText in dogPoundStatusTexts)
        {
            if (statusText != null && statusText.gameObject.activeSelf && statusText.enabled)
            {
                dogPoundStatusText = statusText;
                return;
            }
        }

        dogPoundStatusText = dogPoundStatusTexts.Count > 0 ? dogPoundStatusTexts[0] : null;
    }

    void ConfigurePoundSlotButton(int slotIndex, UnityEngine.Events.UnityAction action)
    {
        if (slotIndex < 0 || slotIndex >= poundDogSelectButtons.Length || poundDogSelectButtons[slotIndex] == null)
        {
            return;
        }

        poundDogSelectButtons[slotIndex].onClick.RemoveListener(action);
        poundDogSelectButtons[slotIndex].onClick.AddListener(action);
    }

    void ConfigurePoundButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    bool EnsureDogManager()
    {
        if (dogManager == null)
        {
            dogManager = GetComponent<DogManager>();
        }

        if (dogManager == null)
        {
            dogManager = FindFirstObjectByType<DogManager>();
        }

        return dogManager != null;
    }

    void LoadOrCreateDogPoundInventory()
    {
        if (!DogPoundSettings.TryLoadInventory(out dogPoundInventory, out dogPoundLastRefreshUtc) ||
            !IsDogPoundInventoryValid(dogPoundInventory))
        {
            GenerateNewDogPoundInventory();
            return;
        }

        if (IsDogPoundRefreshReady())
        {
            GenerateNewDogPoundInventory();
        }
    }

    bool IsDogPoundInventoryValid(DogPoundInventoryData inventory)
    {
        if (inventory == null || inventory.offers == null || inventory.offers.Count != DogPoundSlotCount)
        {
            return false;
        }

        foreach (DogPoundOfferData offer in inventory.offers)
        {
            if (offer == null || offer.dog == null || offer.tokenCost < 1 ||
                string.IsNullOrWhiteSpace(offer.dog.dogId) ||
                string.IsNullOrWhiteSpace(offer.dog.dogName) ||
                string.IsNullOrWhiteSpace(offer.dog.breed))
            {
                return false;
            }
        }

        return dogPoundLastRefreshUtc != default;
    }

    void GenerateNewDogPoundInventory()
    {
        if (!EnsureDogManager())
        {
            return;
        }

        DogPoundInventoryData newInventory = new DogPoundInventoryData();
        int generationSeed = UnityEngine.Random.Range(1000, 100000);

        for (int slotIndex = 0; slotIndex < DogPoundSlotCount; slotIndex++)
        {
            DogGender gender = slotIndex == 1 ? DogGender.Female : DogGender.Male;
            Dog poundDog = dogManager.CreateDogPoundCandidate(generationSeed + slotIndex, gender);

            if (poundDog == null)
            {
                Debug.LogWarning("Dog Pound could not generate a dog offer.");
                return;
            }

            newInventory.offers.Add(new DogPoundOfferData
            {
                dog = dogManager.CreateDogSaveData(poundDog),
                tokenCost = CalculateDogPoundTokenCost(poundDog),
                adopted = false
            });
        }

        dogPoundInventory = newInventory;
        dogPoundLastRefreshUtc = DateTime.UtcNow;
        SaveDogPoundInventory();
    }

    void SaveDogPoundInventory()
    {
        if (dogPoundInventory == null || dogPoundLastRefreshUtc == default)
        {
            return;
        }

        DogPoundSettings.SaveInventory(dogPoundInventory, dogPoundLastRefreshUtc.Ticks);
    }

    int CalculateDogPoundTokenCost(Dog dog)
    {
        if (dog == null)
        {
            return 2;
        }

        int cost = 2;
        int potentialScore = dog.GetPotentialScore();

        if (potentialScore >= 85)
        {
            cost++;
        }

        if (potentialScore >= 100)
        {
            cost++;
        }

        if (dog.primaryTrait != DogTrait.None || dog.secondaryTrait != DogTrait.None)
        {
            cost++;
        }

        return Mathf.Clamp(cost, 2, 5);
    }

    void SelectPoundDogSlot(int slotIndex)
    {
        LoadOrCreateDogPoundInventory();
        Debug.Log($"Dog Pound slot selected: {slotIndex}. Tokens: {DogPoundSettings.TokenCount}.");

        if (!TryGetAvailablePoundOffer(slotIndex, out DogPoundOfferData offer))
        {
            RefreshDogPoundUI();
            SetDogPoundStatus("Selected dog data is missing.");
            Debug.LogWarning($"Dog Pound selection failed. Slot {slotIndex} is unavailable or missing data.");
            return;
        }

        selectedPoundDogSlot = slotIndex;
        string dogName = string.IsNullOrWhiteSpace(offer.dog.dogName) ? "Dog" : offer.dog.dogName;
        Debug.Log($"Dog Pound selected dog cost: {offer.tokenCost}. Token count: {DogPoundSettings.TokenCount}.");
        RefreshDogPoundUI();
        SetDogPoundStatus($"Selected {dogName}.");
    }

    bool TryGetAvailablePoundOffer(int slotIndex, out DogPoundOfferData offer)
    {
        offer = null;

        if (dogPoundInventory == null || dogPoundInventory.offers == null ||
            slotIndex < 0 || slotIndex >= dogPoundInventory.offers.Count)
        {
            return false;
        }

        DogPoundOfferData candidate = dogPoundInventory.offers[slotIndex];

        if (candidate == null || candidate.adopted || candidate.dog == null)
        {
            return false;
        }

        offer = candidate;
        return true;
    }

    void RefreshDogPoundUI()
    {
        BindDogPoundSceneReferencesIfMissing();

        if (dogPoundTokenText != null)
        {
            dogPoundTokenText.text = $"Dog Pound Tokens: {DogPoundSettings.TokenCount}";
        }

        UpdateDogPoundTimerText();

        for (int slotIndex = 0; slotIndex < DogPoundSlotCount; slotIndex++)
        {
            DogPoundOfferData offer = GetPoundOffer(slotIndex);
            bool isAvailable = offer != null && !offer.adopted && offer.dog != null;

            if (slotIndex < poundDogSlots.Length && poundDogSlots[slotIndex] != null)
            {
                poundDogSlots[slotIndex].SetActive(isAvailable);
            }

            if (isAvailable)
            {
                SetPoundDogSlotText(slotIndex, offer);
            }

            if (slotIndex < poundDogSelectButtons.Length && poundDogSelectButtons[slotIndex] != null)
            {
                poundDogSelectButtons[slotIndex].interactable = isAvailable;
            }
        }

        if (adoptButton != null)
        {
            // Leave the button clickable so the player receives a clear reason when adoption cannot proceed.
            adoptButton.interactable = true;
        }

        UpdatePoundDogSlotSelectionVisuals();

        if (!HasAvailablePoundDogs())
        {
            SetDogPoundStatus($"All pound dogs adopted. New dogs arrive in {GetDogPoundRefreshRemainingText()}.");
        }
    }

    DogPoundOfferData GetPoundOffer(int slotIndex)
    {
        if (dogPoundInventory == null || dogPoundInventory.offers == null ||
            slotIndex < 0 || slotIndex >= dogPoundInventory.offers.Count)
        {
            return null;
        }

        return dogPoundInventory.offers[slotIndex];
    }

    void SetPoundDogSlotText(int slotIndex, DogPoundOfferData offer)
    {
        DogSaveData dog = offer.dog;
        SetTextAtIndex(poundDogNameTexts, slotIndex, string.IsNullOrWhiteSpace(dog.dogName) ? "Unknown Dog" : dog.dogName);
        SetTextAtIndex(poundDogBreedTexts, slotIndex, $"Breed: {GetDisplayText(dog.breed, "Unknown Breed")}");
        SetTextAtIndex(poundDogSexTexts, slotIndex, $"Sex: {dog.gender}");
        SetTextAtIndex(poundDogStatsTexts, slotIndex,
            $"STR {dog.strength}  AGI {dog.agility}\nSTA {dog.stamina}  INT {GetSavedDogIntelligence(dog)}");
        SetTextAtIndex(poundDogStrategyTexts, slotIndex, $"Style: {dog.fightStyle}");
        SetTextAtIndex(poundDogTraitsTexts, slotIndex, $"Traits: {GetSavedDogTraitSummary(dog)}");
        SetTextAtIndex(poundDogCostTexts, slotIndex, $"Cost: {offer.tokenCost} Tokens");
    }

    void SetTextAtIndex(TextMeshProUGUI[] texts, int index, string value)
    {
        if (texts != null && index >= 0 && index < texts.Length && texts[index] != null)
        {
            texts[index].text = value;
        }
    }

    void UpdatePoundDogSlotSelectionVisuals()
    {
        for (int slotIndex = 0; slotIndex < DogPoundSlotCount; slotIndex++)
        {
            if (slotIndex >= poundDogSlots.Length || poundDogSlots[slotIndex] == null)
            {
                continue;
            }

            Image slotBackground = poundDogSlots[slotIndex].GetComponent<Image>();

            if (slotBackground == null)
            {
                continue;
            }

            if (!poundDogSlotBaseColorsCaptured[slotIndex])
            {
                poundDogSlotBaseColors[slotIndex] = slotBackground.color;
                poundDogSlotBaseColorsCaptured[slotIndex] = true;
            }

            bool isSelected = selectedPoundDogSlot == slotIndex &&
                              TryGetAvailablePoundOffer(slotIndex, out _);
            slotBackground.color = isSelected
                ? Color.Lerp(poundDogSlotBaseColors[slotIndex], new Color(0.2f, 0.9f, 1f, 1f), 0.55f)
                : poundDogSlotBaseColors[slotIndex];
        }
    }

    int GetSavedDogIntelligence(DogSaveData dog)
    {
        return dog.intelligence > 0 ? dog.intelligence : Dog.DefaultIntelligence;
    }

    string GetSavedDogTraitSummary(DogSaveData dog)
    {
        bool hasPrimary = dog.primaryTrait != DogTrait.None;
        bool hasSecondary = dog.secondaryTrait != DogTrait.None;

        if (!hasPrimary && !hasSecondary)
        {
            return "No Traits";
        }

        if (!hasPrimary || dog.primaryTrait == dog.secondaryTrait)
        {
            return dog.secondaryTrait.ToString();
        }

        if (!hasSecondary)
        {
            return dog.primaryTrait.ToString();
        }

        return $"{dog.primaryTrait} / {dog.secondaryTrait}";
    }

    string GetDisplayText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    bool HasAvailablePoundDogs()
    {
        if (dogPoundInventory == null || dogPoundInventory.offers == null)
        {
            return false;
        }

        foreach (DogPoundOfferData offer in dogPoundInventory.offers)
        {
            if (offer != null && !offer.adopted && offer.dog != null)
            {
                return true;
            }
        }

        return false;
    }

    bool IsDogPoundRefreshReady()
    {
        return dogPoundLastRefreshUtc == default || DateTime.UtcNow >= dogPoundLastRefreshUtc + DogPoundRefreshPeriod;
    }

    string GetDogPoundRefreshRemainingText()
    {
        TimeSpan remaining = dogPoundLastRefreshUtc + DogPoundRefreshPeriod - DateTime.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            return "0h 0m";
        }

        int totalMinutes = Mathf.CeilToInt((float)remaining.TotalMinutes);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }

    void UpdateDogPoundTimerText()
    {
        if (dogPoundTimerText == null || dogPoundLastRefreshUtc == default)
        {
            return;
        }

        dogPoundTimerText.text = IsDogPoundRefreshReady()
            ? "New pound dogs are ready."
            : $"New dogs in {GetDogPoundRefreshRemainingText()}";
    }

    void SetDogPoundStatus(string message)
    {
        foreach (TextMeshProUGUI statusText in dogPoundStatusTexts)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        if (dogPoundStatusTexts.Count == 0 && dogPoundStatusText != null)
        {
            dogPoundStatusText.text = message;
        }
    }

    void ReportDogPoundAdoptionFailure(string message)
    {
        SetDogPoundStatus(message);
        Debug.LogWarning($"Dog Pound adoption failed. {message} Selected slot: {selectedPoundDogSlot}. Tokens: {DogPoundSettings.TokenCount}.");
    }

    void HideNarratorForDogPound()
    {
        if (narratorPanel == null)
        {
            return;
        }

        narratorWasVisibleBeforeDogPound = narratorPanel.activeSelf;
        narratorPanel.SetActive(false);
    }

    void RestoreNarratorAfterDogPound()
    {
        if (narratorPanel != null && narratorWasVisibleBeforeDogPound)
        {
            narratorPanel.SetActive(true);
        }

        narratorWasVisibleBeforeDogPound = false;
    }

    GameObject FindSceneObjectByTrimmedName(string objectName)
    {
        foreach (GameObject sceneObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (sceneObject.scene.IsValid() && string.Equals(sceneObject.name.Trim(), objectName, StringComparison.Ordinal))
            {
                return sceneObject;
            }
        }

        return null;
    }

    GameObject FindGameObjectInTree(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(child.name.Trim(), objectName, StringComparison.Ordinal))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    TextMeshProUGUI FindTextInTree(Transform root, string objectName)
    {
        GameObject textObject = FindGameObjectInTree(root, objectName);
        return textObject != null ? textObject.GetComponent<TextMeshProUGUI>() : null;
    }

    Button FindButtonInTree(Transform root, params string[] objectNames)
    {
        if (root == null)
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            foreach (string objectName in objectNames)
            {
                if (string.Equals(child.name.Trim(), objectName, StringComparison.Ordinal))
                {
                    return child.GetComponent<Button>();
                }
            }
        }

        return null;
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

public static class DogPoundSettings
{
    public const string TokenCountKey = "DigitalDogArena.DogPound.Tokens";
    public const string InventoryKey = "DigitalDogArena.DogPound.Inventory";
    public const string LastRefreshUtcTicksKey = "DigitalDogArena.DogPound.LastRefreshUtcTicks";

    public static int TokenCount
    {
        get
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(TokenCountKey, 0));
        }
        set
        {
            PlayerPrefs.SetInt(TokenCountKey, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }

    public static void AddTokens(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        TokenCount += amount;
    }

    public static bool TryLoadInventory(out DogPoundInventoryData inventory, out DateTime lastRefreshUtc)
    {
        inventory = null;
        lastRefreshUtc = default;

        if (!PlayerPrefs.HasKey(InventoryKey) || !PlayerPrefs.HasKey(LastRefreshUtcTicksKey))
        {
            return false;
        }

        try
        {
            string json = PlayerPrefs.GetString(InventoryKey, string.Empty);
            string savedTicks = PlayerPrefs.GetString(LastRefreshUtcTicksKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json) || !long.TryParse(savedTicks, out long utcTicks))
            {
                return false;
            }

            inventory = JsonUtility.FromJson<DogPoundInventoryData>(json);
            lastRefreshUtc = new DateTime(utcTicks, DateTimeKind.Utc);
            return inventory != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Dog Pound save data was invalid and will be regenerated. {exception.Message}");
            inventory = null;
            lastRefreshUtc = default;
            return false;
        }
    }

    public static void SaveInventory(DogPoundInventoryData inventory, long lastRefreshUtcTicks)
    {
        if (inventory == null || lastRefreshUtcTicks <= 0)
        {
            return;
        }

        PlayerPrefs.SetString(InventoryKey, JsonUtility.ToJson(inventory));
        PlayerPrefs.SetString(LastRefreshUtcTicksKey, lastRefreshUtcTicks.ToString());
        PlayerPrefs.Save();
    }
}

[Serializable]
public class DogPoundInventoryData
{
    public List<DogPoundOfferData> offers = new List<DogPoundOfferData>();
}

[Serializable]
public class DogPoundOfferData
{
    public DogSaveData dog;
    public int tokenCost;
    public bool adopted;
}
