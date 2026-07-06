using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class DogManager : MonoBehaviour
{
    private const string SaveKey = "DOG_STABLE_SAVE";
    private const string StarterStableSeedKey = "DOG_STARTER_STABLE_SEEDED";
    private const int TargetStarterStableSize = 14;
    private const int MinimumStarterMales = 5;
    private const int MinimumStarterFemales = 5;
    private const int StarterStatMin = 8;
    private const int StarterStatMax = 22;
    private const int StarterPotentialMin = 60;
    private const int StarterPotentialMax = 105;
    private const string StableFilterAllDogs = "All Dogs";

    private static readonly string[] generatedStarterMaleNamePool =
    {
        "Titan",
        "Diesel",
        "Knox",
        "Ghost",
        "Rocco",
        "Atlas",
        "Kane",
        "Blitz",
        "Rex",
        "Duke",
        "Thor",
        "Apollo",
        "Odin",
        "Jax",
        "Axel",
        "Drake"
    };

    private static readonly string[] generatedStarterFemaleNamePool =
    {
        "Luna",
        "Nova",
        "Sable",
        "Kira",
        "Juno",
        "Vexa",
        "Nyx",
        "Rogue",
        "Raven",
        "Xena",
        "Athena",
        "Roxy",
        "Bella",
        "Nala",
        "Ruby",
        "Jade",
        "Hera"
    };

    private static readonly string[] generatedStarterNeutralNamePool =
    {
        "Shadow",
        "Storm",
        "Viper",
        "Phoenix",
        "Onyx",
        "Echo",
        "Scout",
        "Ember",
        "Frost",
        "Havoc",
        "Phantom"
    };

    private static readonly string[] generatedStarterNameSuffixPool =
    {
        "Fang",
        "Storm",
        "Ghost",
        "Bolt",
        "Rogue",
        "Blitz"
    };

    private static readonly string[] stableBreedFilterOptions =
    {
        StableFilterAllDogs,
        "Bully Striker / Pit Types",
        "Iron Rott / Rott Types",
        "Guard Mastiff / Mastiff Types",
        "Shepherd Sentinel / Shepherd Types",
        "Spitz Warden / Spitz Types",
        "Velocity Hound / Hound Types",
        "Hybrids",
        "Pit Bull",
        "Rottweiler",
        "German Shepherd",
        "Mastiff",
        "Cane Corso",
        "Doberman",
        "Belgian Malinois",
        "Akita",
        "Greyhound"
    };

    [Header("Owned Dogs")]
    public List<Dog> ownedDogs = new List<Dog>();

    [Header("Game Time")]
    public int currentWeek = 1;

    [Header("UI")]
    public Transform dogContainer;
    public GameObject dogCardPrefab;
    public TextMeshProUGUI selectedFightersText;
    public TextMeshProUGUI matchupPreviewText;
    public TMP_Dropdown stableBreedFilterDropdown;

    [Header("Economy")]
    public EconomyManager economyManager;
    public NarratorManager narratorManager;

    [Header("Dog Selection Dropdowns")]
    public TMP_Dropdown fighter1DogDropdown;
    public TMP_Dropdown fighter2DogDropdown;
    public TMP_Dropdown parent1DogDropdown;
    public TMP_Dropdown parent2DogDropdown;

    [Header("Strategy Dropdowns")]
    public TMP_Dropdown fighter1StrategyDropdown;
    public TMP_Dropdown fighter2StrategyDropdown;

    [Header("Selected Fighters")]
    public Dog selectedFighter1;
    public Dog selectedFighter2;

    [Header("Selected Parents")]
    public Dog selectedParent1;
    public Dog selectedParent2;

    [Header("Selected Strategies")]
    public FightStrategy fighter1Strategy = FightStrategy.Balanced;
    public FightStrategy fighter2Strategy = FightStrategy.Balanced;

    private List<Dog> selectableFighterDogs = new List<Dog>();
    private List<Dog> selectableParentDogs = new List<Dog>();
    private List<Dog> selectableParent1Dogs = new List<Dog>();
    private List<Dog> selectableParent2Dogs = new List<Dog>();
    private bool isRefreshingDogDropdowns = false;
    private int selectedStableBreedFilterIndex = 0;
    private DogDetailPanelUI dogDetailPanel;

    void Start()
    {
        if (economyManager == null)
        {
            economyManager = GetComponent<EconomyManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = GetComponent<NarratorManager>();
        }

        if (HasStableSave())
        {
            LoadStable();
            SeedStarterStableIfNeeded();
        }
        else
        {
            LoadStartingDogs();
            TopUpStarterStable();
            SaveStable();
            MarkStarterStableSeeded();
        }

        SetupStrategyDropdowns();
        RefreshDogSelectionDropdowns();
        SetupStableBreedFilterDropdown();

        DisplayDogs();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();
    }

    void LoadStartingDogs()
    {
        ownedDogs.Clear();

        AddDogFromResources("Spike", "spike");
        AddDogFromResources("Rex", "rex");
        AddDogFromResources("Luna", "luna");

        if (ownedDogs.Count == 0)
        {
            Debug.LogError("Dogs not found! Make sure Spike, Rex, and Luna are inside Assets/Resources.");
        }
    }

    bool HasStableSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    bool HasStarterStableSeed()
    {
        return PlayerPrefs.GetInt(StarterStableSeedKey, 0) == 1;
    }

    void MarkStarterStableSeeded()
    {
        PlayerPrefs.SetInt(StarterStableSeedKey, 1);
        PlayerPrefs.Save();
    }

    void SeedStarterStableIfNeeded()
    {
        if (HasStarterStableSeed())
        {
            return;
        }

        bool needsMoreDogs = ownedDogs.Count < TargetStarterStableSize;
        bool needsMoreMales = CountGender(DogGender.Male) < MinimumStarterMales;
        bool needsMoreFemales = CountGender(DogGender.Female) < MinimumStarterFemales;

        if (needsMoreDogs || needsMoreMales || needsMoreFemales)
        {
            TopUpStarterStable();
            SaveStable();
        }

        MarkStarterStableSeeded();
    }

    void AddDogFromResources(string dogResourceName, string fallbackId)
    {
        Dog dogTemplate = Resources.Load<Dog>(dogResourceName);

        if (dogTemplate == null)
        {
            Debug.LogWarning($"Dog not found in Resources: {dogResourceName}");
            return;
        }

        Dog dogInstance = Instantiate(dogTemplate);

        if (string.IsNullOrEmpty(dogInstance.dogId) || dogInstance.dogId == "dog_id")
        {
            dogInstance.dogId = fallbackId;
        }

        dogInstance.NormalizeLegacyStats();
        ApplyStartingGenderFallback(dogInstance, fallbackId);

        ownedDogs.Add(dogInstance);
    }

    void TopUpStarterStable()
    {
        int generatedCount = 0;

        while (CountGender(DogGender.Male) < MinimumStarterMales)
        {
            AddGeneratedStarterDog(DogGender.Male, generatedCount);
            generatedCount++;
        }

        while (CountGender(DogGender.Female) < MinimumStarterFemales)
        {
            AddGeneratedStarterDog(DogGender.Female, generatedCount);
            generatedCount++;
        }

        while (ownedDogs.Count < TargetStarterStableSize)
        {
            DogGender gender = CountGender(DogGender.Male) <= CountGender(DogGender.Female)
                ? DogGender.Male
                : DogGender.Female;

            AddGeneratedStarterDog(gender, generatedCount);
            generatedCount++;
        }

        if (generatedCount > 0)
        {
            Debug.Log($"Generated {generatedCount} starter dogs. Stable now has {ownedDogs.Count} dogs.");
        }
    }

    void AddGeneratedStarterDog(DogGender gender, int generatedIndex)
    {
        Dog generatedDog = CreateGeneratedStarterDog(gender, generatedIndex);

        if (generatedDog != null)
        {
            ownedDogs.Add(generatedDog);
        }
    }

    Dog CreateGeneratedStarterDog(DogGender gender, int generatedIndex)
    {
        Dog dog = ScriptableObject.CreateInstance<Dog>();
        string breedName = ChooseStarterBreedName(generatedIndex);
        BreedLibrary.BreedInfo breedInfo = GetBreedInfoOrDefault(breedName);

        dog.dogId = $"starter_generated_{System.Guid.NewGuid().ToString("N")}";
        dog.dogName = GetUniqueStarterDogName(gender, generatedIndex);
        dog.breed = breedName;
        dog.gender = gender;
        dog.age = 0;
        dog.isDead = false;
        dog.isRetired = false;
        dog.lastBredWeek = -999;

        dog.strength = GenerateStarterStat(breedInfo.strengthBias);
        dog.agility = GenerateStarterStat(breedInfo.agilityBias);
        dog.stamina = GenerateStarterStat(breedInfo.staminaBias);
        dog.intelligence = GenerateStarterStat(breedInfo.intelligenceBias);

        dog.strengthPotential = GenerateStarterPotential(dog.strength, breedInfo.strengthBias);
        dog.agilityPotential = GenerateStarterPotential(dog.agility, breedInfo.agilityBias);
        dog.staminaPotential = GenerateStarterPotential(dog.stamina, breedInfo.staminaBias);
        dog.intelligencePotential = GenerateStarterPotential(dog.intelligence, breedInfo.intelligenceBias);

        dog.growthRate = Random.Range(0.9f, 1.11f);
        dog.fightStyle = ChooseStarterFightStyle(breedInfo.styleTendency);
        AssignStarterTraits(dog);

        dog.level = 1;
        dog.xp = 0;
        dog.xpToNextLevel = 100;
        dog.wins = 0;
        dog.losses = 0;
        dog.totalFights = 0;

        dog.NormalizeLegacyStats();
        return dog;
    }

    string ChooseStarterBreedName(int generatedIndex)
    {
        List<string> breedNames = BreedLibrary.GetBaseBreedNames();

        if (breedNames == null || breedNames.Count == 0)
        {
            return "Pit Bull";
        }

        int breedIndex = Mathf.Abs((generatedIndex * 7) + Random.Range(0, breedNames.Count)) % breedNames.Count;
        return breedNames[breedIndex];
    }

    BreedLibrary.BreedInfo GetBreedInfoOrDefault(string breedName)
    {
        if (BreedLibrary.TryGetBaseBreed(breedName, out BreedLibrary.BreedInfo breedInfo))
        {
            return breedInfo;
        }

        return new BreedLibrary.BreedInfo(
            "Pit Bull",
            3,
            1,
            2,
            1,
            FightStyle.Rushdown,
            "Compact, muscular, and explosive.");
    }

    int GenerateStarterStat(int breedBias)
    {
        int baseStat = Random.Range(11, 17);
        int variance = Random.Range(-2, 3);
        return Mathf.Clamp(baseStat + breedBias + variance, StarterStatMin, StarterStatMax);
    }

    int GenerateStarterPotential(int currentStat, int breedBias)
    {
        int potential = currentStat + Random.Range(55, 78) + Mathf.Max(0, breedBias * 2);
        return Mathf.Clamp(potential, StarterPotentialMin, StarterPotentialMax);
    }

    FightStyle ChooseStarterFightStyle(FightStyle breedStyle)
    {
        if (Random.value < 0.75f)
        {
            return breedStyle;
        }

        int styleCount = System.Enum.GetValues(typeof(FightStyle)).Length;
        return (FightStyle)Random.Range(0, styleCount);
    }

    void AssignStarterTraits(Dog dog)
    {
        if (dog == null)
        {
            return;
        }

        dog.primaryTrait = DogTrait.None;
        dog.secondaryTrait = DogTrait.None;

        float traitRoll = Random.value;

        if (traitRoll < 0.62f)
        {
            return;
        }

        dog.primaryTrait = GetRandomStarterTrait();

        if (traitRoll > 0.92f)
        {
            DogTrait secondTrait = GetRandomStarterTrait();

            if (secondTrait != dog.primaryTrait)
            {
                dog.secondaryTrait = secondTrait;
            }
        }
    }

    DogTrait GetRandomStarterTrait()
    {
        DogTrait[] traits =
        {
            DogTrait.Aggressive,
            DogTrait.Durable,
            DogTrait.GlassCannon,
            DogTrait.Clutch,
            DogTrait.LateBloomer,
            DogTrait.Prodigy
        };

        return traits[Random.Range(0, traits.Length)];
    }

    string GetUniqueStarterDogName(DogGender gender, int generatedIndex)
    {
        string[] primaryPool = gender == DogGender.Female
            ? generatedStarterFemaleNamePool
            : generatedStarterMaleNamePool;

        string candidate = GetUniqueNameFromPool(primaryPool, generatedIndex);

        if (!string.IsNullOrEmpty(candidate))
        {
            return candidate;
        }

        candidate = GetUniqueNameFromPool(generatedStarterNeutralNamePool, generatedIndex);

        if (!string.IsNullOrEmpty(candidate))
        {
            return candidate;
        }

        candidate = GetUniqueSuffixedNameFromPool(primaryPool, generatedIndex);

        if (!string.IsNullOrEmpty(candidate))
        {
            return candidate;
        }

        candidate = GetUniqueSuffixedNameFromPool(generatedStarterNeutralNamePool, generatedIndex);

        if (!string.IsNullOrEmpty(candidate))
        {
            return candidate;
        }

        return gender == DogGender.Female ? "Nova" : "Titan";
    }

    string GetUniqueNameFromPool(string[] namePool, int generatedIndex)
    {
        if (namePool == null || namePool.Length == 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < namePool.Length; i++)
        {
            string candidate = namePool[(generatedIndex + i) % namePool.Length];

            if (!HasDogName(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    string GetUniqueSuffixedNameFromPool(string[] namePool, int generatedIndex)
    {
        if (namePool == null || namePool.Length == 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < namePool.Length; i++)
        {
            string candidate =
                namePool[(generatedIndex + i) % namePool.Length] +
                generatedStarterNameSuffixPool[(generatedIndex + i) % generatedStarterNameSuffixPool.Length];

            if (!HasDogName(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    bool HasDogName(string dogName)
    {
        for (int i = 0; i < ownedDogs.Count; i++)
        {
            Dog dog = ownedDogs[i];

            if (dog != null && string.Equals(dog.dogName, dogName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    int CountGender(DogGender gender)
    {
        int count = 0;

        for (int i = 0; i < ownedDogs.Count; i++)
        {
            Dog dog = ownedDogs[i];

            if (dog != null && dog.gender == gender)
            {
                count++;
            }
        }

        return count;
    }

    void ApplyStartingGenderFallback(Dog dog, string fallbackId)
    {
        if (dog == null) return;

        switch (fallbackId)
        {
            case "luna":
                dog.gender = DogGender.Female;
                break;

            case "spike":
            case "rex":
                dog.gender = DogGender.Male;
                break;
        }
    }

    public void DisplayDogs()
    {
        if (dogContainer == null)
        {
            Debug.LogError("Dog Container is not assigned on DogManager.");
            return;
        }

        if (dogCardPrefab == null)
        {
            Debug.LogError("Dog Card Prefab is not assigned on DogManager.");
            return;
        }

        foreach (Transform child in dogContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Dog dog in ownedDogs)
        {
            if (dog == null) continue;
            if (!DogMatchesStableFilter(dog, GetSelectedStableBreedFilter())) continue;

            GameObject card = Instantiate(dogCardPrefab, dogContainer);
            DogCardUI cardUI = card.GetComponent<DogCardUI>();

            if (cardUI == null)
            {
                Debug.LogError("Dog Card Prefab is missing the DogCardUI script.");
                continue;
            }

            cardUI.Setup(dog, this);
        }
    }

    void SetupStableBreedFilterDropdown()
    {
        if (stableBreedFilterDropdown == null)
        {
            stableBreedFilterDropdown = FindExistingStableBreedFilterDropdown();
        }

        if (stableBreedFilterDropdown == null)
        {
            stableBreedFilterDropdown = CreateStableBreedFilterDropdown();
        }

        if (stableBreedFilterDropdown == null)
        {
            Debug.LogWarning("Stable breed filter dropdown could not be created.");
            return;
        }

        selectedStableBreedFilterIndex = Mathf.Clamp(
            selectedStableBreedFilterIndex,
            0,
            stableBreedFilterOptions.Length - 1);

        stableBreedFilterDropdown.ClearOptions();
        stableBreedFilterDropdown.AddOptions(new List<string>(stableBreedFilterOptions));
        stableBreedFilterDropdown.SetValueWithoutNotify(selectedStableBreedFilterIndex);
        stableBreedFilterDropdown.RefreshShownValue();
        stableBreedFilterDropdown.onValueChanged.RemoveListener(OnStableBreedFilterChanged);
        stableBreedFilterDropdown.onValueChanged.AddListener(OnStableBreedFilterChanged);
    }

    TMP_Dropdown FindExistingStableBreedFilterDropdown()
    {
        Transform stablePage = FindStablePageTransform();

        if (stablePage == null)
        {
            return null;
        }

        Transform dropdownTransform = FindChildByName(stablePage, "StableBreedFilterDropdown");
        return dropdownTransform != null
            ? dropdownTransform.GetComponent<TMP_Dropdown>()
            : null;
    }

    TMP_Dropdown CreateStableBreedFilterDropdown()
    {
        Transform stablePage = FindStablePageTransform();
        TMP_Dropdown sourceDropdown = GetStableFilterDropdownTemplateSource();

        if (stablePage == null || sourceDropdown == null)
        {
            return null;
        }

        Transform filterArea = CreateOrFindStableFilterArea(stablePage);
        CreateOrUpdateStableFilterLabel(filterArea);

        GameObject dropdownObject = Instantiate(sourceDropdown.gameObject, filterArea);
        dropdownObject.name = "StableBreedFilterDropdown";
        dropdownObject.SetActive(true);

        RectTransform dropdownRect = dropdownObject.GetComponent<RectTransform>();
        if (dropdownRect != null)
        {
            dropdownRect.anchorMin = new Vector2(0f, 1f);
            dropdownRect.anchorMax = new Vector2(0f, 1f);
            dropdownRect.pivot = new Vector2(0f, 1f);
            dropdownRect.anchoredPosition = new Vector2(92f, -4f);
            dropdownRect.sizeDelta = new Vector2(360f, 38f);
        }

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();

        if (dropdown != null)
        {
            dropdown.interactable = true;
            dropdown.onValueChanged.RemoveAllListeners();
        }

        return dropdown;
    }

    Transform CreateOrFindStableFilterArea(Transform stablePage)
    {
        Transform existingArea = FindChildByName(stablePage, "StableFilterArea");

        if (existingArea != null)
        {
            return existingArea;
        }

        GameObject areaObject = new GameObject("StableFilterArea", typeof(RectTransform));
        areaObject.transform.SetParent(stablePage, false);

        RectTransform areaRect = areaObject.GetComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0f, 1f);
        areaRect.anchorMax = new Vector2(0f, 1f);
        areaRect.pivot = new Vector2(0f, 1f);
        areaRect.anchoredPosition = new Vector2(70f, -150f);
        areaRect.sizeDelta = new Vector2(470f, 46f);

        return areaObject.transform;
    }

    void CreateOrUpdateStableFilterLabel(Transform filterArea)
    {
        Transform existingLabel = FindChildByName(filterArea, "StableBreedFilterLabel");
        TextMeshProUGUI labelText;

        if (existingLabel != null)
        {
            labelText = existingLabel.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            GameObject labelObject = new GameObject("StableBreedFilterLabel", typeof(RectTransform));
            labelObject.transform.SetParent(filterArea, false);
            labelText = labelObject.AddComponent<TextMeshProUGUI>();
        }

        if (labelText == null)
        {
            return;
        }

        labelText.text = "Filter";
        labelText.fontSize = 24f;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.MidlineRight;
        labelText.raycastTarget = false;

        TextMeshProUGUI stableTitleText = FindStableTitleText();
        if (stableTitleText != null)
        {
            labelText.font = stableTitleText.font;
            labelText.fontSharedMaterial = stableTitleText.fontSharedMaterial;
        }

        RectTransform labelRect = labelText.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(0f, -4f);
        labelRect.sizeDelta = new Vector2(82f, 38f);
    }

    TMP_Dropdown GetStableFilterDropdownTemplateSource()
    {
        if (fighter1DogDropdown != null) return fighter1DogDropdown;
        if (fighter2DogDropdown != null) return fighter2DogDropdown;
        if (parent1DogDropdown != null) return parent1DogDropdown;
        if (parent2DogDropdown != null) return parent2DogDropdown;
        if (fighter1StrategyDropdown != null) return fighter1StrategyDropdown;
        if (fighter2StrategyDropdown != null) return fighter2StrategyDropdown;

        return null;
    }

    Transform FindStablePageTransform()
    {
        GameObject stablePageObject = GameObject.Find("StablePage");
        return stablePageObject != null ? stablePageObject.transform : null;
    }

    TextMeshProUGUI FindStableTitleText()
    {
        Transform stablePage = FindStablePageTransform();

        if (stablePage == null)
        {
            return null;
        }

        Transform stableTitle = FindChildByName(stablePage, "StableTitle");
        return stableTitle != null ? stableTitle.GetComponent<TextMeshProUGUI>() : null;
    }

    Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
            {
                return children[i];
            }
        }

        return null;
    }

    void OnStableBreedFilterChanged(int optionIndex)
    {
        selectedStableBreedFilterIndex = Mathf.Clamp(optionIndex, 0, stableBreedFilterOptions.Length - 1);
        DisplayDogs();
    }

    string GetSelectedStableBreedFilter()
    {
        if (stableBreedFilterDropdown != null &&
            stableBreedFilterDropdown.options != null &&
            stableBreedFilterDropdown.options.Count > 0)
        {
            int optionIndex = Mathf.Clamp(stableBreedFilterDropdown.value, 0, stableBreedFilterDropdown.options.Count - 1);
            return stableBreedFilterDropdown.options[optionIndex].text;
        }

        return stableBreedFilterOptions[Mathf.Clamp(selectedStableBreedFilterIndex, 0, stableBreedFilterOptions.Length - 1)];
    }

    private bool DogMatchesStableFilter(Dog dog, string selectedFilter)
    {
        if (dog == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(selectedFilter) || selectedFilter == StableFilterAllDogs)
        {
            return true;
        }

        string breed = dog.breed;

        switch (selectedFilter)
        {
            case "Bully Striker / Pit Types":
                return IsBullyStableBreed(breed);

            case "Iron Rott / Rott Types":
                return IsIronRottStableBreed(breed);

            case "Guard Mastiff / Mastiff Types":
                return IsGuardMastiffStableBreed(breed);

            case "Shepherd Sentinel / Shepherd Types":
                return IsShepherdStableBreed(breed);

            case "Spitz Warden / Spitz Types":
                return IsSpitzStableBreed(breed);

            case "Velocity Hound / Hound Types":
                return IsVelocityHoundStableBreed(breed);

            case "Hybrids":
                return IsHybridStableBreed(breed);

            case "Pit Bull":
                return BreedHasAnyText(breed, "pit bull", "pitbull");

            case "Rottweiler":
                return BreedHasAnyText(breed, "rottweiler");

            case "German Shepherd":
                return BreedHasAnyText(breed, "german shepherd", "german shepard");

            case "Mastiff":
                return BreedHasAnyText(breed, "mastiff");

            case "Cane Corso":
                return BreedHasAnyText(breed, "cane corso", "canecorso");

            case "Doberman":
                return BreedHasAnyText(breed, "doberman");

            case "Belgian Malinois":
                return BreedHasAnyText(breed, "belgian malinois", "malinois");

            case "Akita":
                return BreedHasAnyText(breed, "akita");

            case "Greyhound":
                return BreedHasAnyText(breed, "greyhound");
        }

        return true;
    }

    bool IsBullyStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "pit bull",
            "pitbull",
            "american bully",
            "american staffordshire",
            "staffordshire terrier",
            "staffordshire",
            "bull terrier",
            "bullterrier",
            "boxer",
            "bulldog",
            "bully",
            "bull",
            "german bull",
            "shepherd bull",
            "pit german",
            "pit shepherd",
            "bull shepherd",
            "bullhound",
            "presabull",
            "bullentino",
            "bullakita",
            "corso bull");
    }

    bool IsIronRottStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "rottweiler",
            "rott",
            "rotterman",
            "doberrott",
            "doberman",
            "beauceron",
            "black russian terrier",
            "rott mastiff",
            "rottmastiff",
            "mastweiler",
            "boxweiler",
            "corso rott",
            "shepherd rott");
    }

    bool IsGuardMastiffStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "mastiff",
            "cane corso",
            "canecorso",
            "presa canario",
            "presa",
            "dogo argentino",
            "dogo",
            "boerboel",
            "great dane",
            "greatdane",
            "kangal",
            "tosa inu",
            "tosa",
            "fila brasileiro",
            "fila",
            "central asian shepherd",
            "rott mastiff",
            "rottmastiff",
            "mastweiler",
            "boxiff",
            "presacorso",
            "akicorso",
            "dobocorso",
            "shepiff",
            "corso bull",
            "corso rott");
    }

    bool IsShepherdStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "german shepherd",
            "german shepard",
            "belgian malinois",
            "malinois",
            "dutch shepherd",
            "anatolian shepherd",
            "australian shepherd",
            "belgian tervuren",
            "tervuren",
            "shepherd",
            "shepard",
            "german bull",
            "pit german",
            "pit shepherd",
            "bull shepherd",
            "shepherd bull",
            "malishep",
            "shepdogo",
            "shepiff",
            "wolf shepherd",
            "shepherd rott",
            "maliberman");
    }

    bool IsSpitzStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "akita",
            "husky",
            "alaskan malamute",
            "malamute",
            "shiba inu",
            "shiba",
            "chow chow",
            "chow",
            "samoyed",
            "spitz",
            "akita husky",
            "wolf husky",
            "bullakita",
            "akicorso");
    }

    bool IsVelocityHoundStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "greyhound",
            "whippet",
            "saluki",
            "rhodesian ridgeback",
            "thai ridgeback",
            "ridgeback",
            "pharaoh hound",
            "pharaoh",
            "catahoula leopard dog",
            "catahoula",
            "hound",
            "greyberman",
            "bullhound");
    }

    bool IsHybridStableBreed(string breed)
    {
        return BreedHasAnyText(
            breed,
            "hybrid",
            "mix",
            "mixed",
            "cross",
            " x ",
            "germanbull",
            "germanbully",
            "shepherdbull",
            "shepherdbully",
            "pitgerman",
            "pitshepherd",
            "bullshepherd",
            "bullyshepherd",
            "rottmastiff",
            "rottie bull",
            "shepherd rott",
            "corso bull",
            "doberrott",
            "malishep",
            "boxiff",
            "bullentino",
            "presacorso",
            "akicorso",
            "greyberman",
            "shepdogo",
            "bullakita",
            "mastweiler",
            "presabull",
            "dobocorso",
            "boxweiler",
            "shepiff",
            "maliberman",
            "bullhound",
            "wolf shepherd",
            "wolf husky");
    }

    bool BreedHasAnyText(string breed, params string[] searchTexts)
    {
        if (string.IsNullOrWhiteSpace(breed) || searchTexts == null)
        {
            return false;
        }

        string rawBreed = GetRawStableBreedText(breed);
        string separatedBreed = GetSeparatedStableBreedText(breed);
        string compactBreed = GetCompactStableBreedText(breed);

        for (int i = 0; i < searchTexts.Length; i++)
        {
            string searchText = searchTexts[i];

            if (string.IsNullOrWhiteSpace(searchText))
            {
                continue;
            }

            string rawSearch = GetRawStableBreedText(searchText);
            string separatedSearch = GetSeparatedStableBreedText(searchText);
            string compactSearch = GetCompactStableBreedText(searchText);

            if ((!string.IsNullOrEmpty(rawSearch) && rawBreed.Contains(rawSearch)) ||
                (!string.IsNullOrEmpty(separatedSearch) && separatedBreed.Contains(separatedSearch)) ||
                (!string.IsNullOrEmpty(compactSearch) && compactBreed.Contains(compactSearch)))
            {
                return true;
            }
        }

        return false;
    }

    string GetRawStableBreedText(string breed)
    {
        return string.IsNullOrWhiteSpace(breed)
            ? string.Empty
            : breed.Trim().ToLowerInvariant();
    }

    string GetSeparatedStableBreedText(string breed)
    {
        string rawBreed = GetRawStableBreedText(breed);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(rawBreed.Length);
        bool previousWasSpace = false;

        for (int i = 0; i < rawBreed.Length; i++)
        {
            char breedCharacter = rawBreed[i];
            bool isSeparator = char.IsWhiteSpace(breedCharacter) ||
                               breedCharacter == '_' ||
                               breedCharacter == '-' ||
                               breedCharacter == '/' ||
                               breedCharacter == '\\' ||
                               breedCharacter == '\'';

            if (isSeparator)
            {
                if (!previousWasSpace && builder.Length > 0)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(breedCharacter);
            previousWasSpace = false;
        }

        return builder.ToString().Trim();
    }

    string GetCompactStableBreedText(string breed)
    {
        string rawBreed = GetRawStableBreedText(breed);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(rawBreed.Length);

        for (int i = 0; i < rawBreed.Length; i++)
        {
            char breedCharacter = rawBreed[i];

            if (char.IsWhiteSpace(breedCharacter) ||
                breedCharacter == '_' ||
                breedCharacter == '-' ||
                breedCharacter == '/' ||
                breedCharacter == '\\' ||
                breedCharacter == '\'')
            {
                continue;
            }

            builder.Append(breedCharacter);
        }

        return builder.ToString();
    }

    string GetDisplayBreedNameForDropdown(string breedName)
    {
        string compactBreed = GetCompactStableBreedText(breedName);

        switch (compactBreed)
        {
            case "germanbull":
                return "German Bull Hybrid";

            case "germanbully":
                return "German Bully Hybrid";

            case "shepherdbull":
                return "Shepherd Bull Hybrid";

            case "shepherdbully":
                return "Shepherd Bully Hybrid";

            case "pitgerman":
                return "Pit German Hybrid";

            case "pitshepherd":
                return "Pit Shepherd Hybrid";

            case "bullshepherd":
                return "Bull Shepherd Hybrid";

            case "bullyshepherd":
                return "Bully Shepherd Hybrid";

            default:
                return string.IsNullOrWhiteSpace(breedName) ? "Unknown Breed" : breedName.Trim();
        }
    }

    public void SelectParent1(Dog dog)
    {
        if (dog == null)
        {
            selectedParent1 = null;
            RefreshDogSelectionDropdowns();
            return;
        }

        if (!CanBreedDog(dog))
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected as a parent.");
            return;
        }

        if (selectedParent2 == dog)
        {
            Debug.LogWarning($"{dog.dogName} is already selected as Parent 2.");
            RefreshDogSelectionDropdowns();
            return;
        }

        if (selectedParent2 != null && selectedParent2.gender == dog.gender)
        {
            Debug.LogWarning($"{dog.dogName} needs a parent of the opposite gender.");
            RefreshDogSelectionDropdowns();
            return;
        }

        selectedParent1 = dog;
        RefreshDogSelectionDropdowns();
    }

    public void SelectParent2(Dog dog)
    {
        if (dog == null)
        {
            selectedParent2 = null;
            RefreshDogSelectionDropdowns();
            return;
        }

        if (!CanBreedDog(dog))
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected as a parent.");
            return;
        }

        if (selectedParent1 == dog)
        {
            Debug.LogWarning($"{dog.dogName} is already selected as Parent 1.");
            RefreshDogSelectionDropdowns();
            return;
        }

        if (selectedParent1 != null && selectedParent1.gender == dog.gender)
        {
            Debug.LogWarning($"{dog.dogName} needs a parent of the opposite gender.");
            RefreshDogSelectionDropdowns();
            return;
        }

        selectedParent2 = dog;
        RefreshDogSelectionDropdowns();
    }
    public void SelectFighter1(Dog dog)
    {
        if (dog == null)
        {
            selectedFighter1 = null;
            UpdateSelectedFightersText();
            UpdateMatchupPreview();
            RefreshDogSelectionDropdowns();
            return;
        }

        if (!CanFightDog(dog))
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected.");
            return;
        }

        if (selectedFighter2 == dog)
        {
            Debug.LogWarning($"{dog.dogName} is already selected as Fighter/Parent 2.");
            UpdateSelectedFightersText();
            UpdateMatchupPreview();
            RefreshDogSelectionDropdowns();
            return;
        }

        selectedFighter1 = dog;

        UpdateSelectedFightersText();
        UpdateMatchupPreview();
        RefreshDogSelectionDropdowns();
    }

    public void SelectFighter2(Dog dog)
    {
        if (dog == null)
        {
            selectedFighter2 = null;
            UpdateSelectedFightersText();
            UpdateMatchupPreview();
            RefreshDogSelectionDropdowns();
            return;
        }

        if (!CanFightDog(dog))
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected.");
            return;
        }

        if (selectedFighter1 == dog)
        {
            Debug.LogWarning($"{dog.dogName} is already selected as Fighter/Parent 1.");
            UpdateSelectedFightersText();
            UpdateMatchupPreview();
            RefreshDogSelectionDropdowns();
            return;
        }

        selectedFighter2 = dog;

        UpdateSelectedFightersText();
        UpdateMatchupPreview();
        RefreshDogSelectionDropdowns();
    }

    bool CanFightDog(Dog dog)
    {
        if (dog == null) return false;

        return dog.CanFight();
    }

    bool CanBreedDog(Dog dog)
    {
        if (dog == null) return false;

        return dog.CanBreedInWeek(currentWeek);
    }

    public void RefreshDogSelectionDropdowns()
    {
        isRefreshingDogDropdowns = true;

        selectableFighterDogs.Clear();
        selectableParentDogs.Clear();

        foreach (Dog dog in ownedDogs)
        {
            if (CanFightDog(dog))
            {
                selectableFighterDogs.Add(dog);
            }

            if (CanBreedDog(dog))
            {
                selectableParentDogs.Add(dog);
            }
        }

        ClearInvalidSelections();

        List<string> fighterOptions = BuildDogDropdownOptions(selectableFighterDogs);
        BuildCompatibleParentDogLists();
        List<string> parent1Options = BuildParentDogDropdownOptions(selectableParent1Dogs);
        List<string> parent2Options = BuildParentDogDropdownOptions(selectableParent2Dogs);

        ConfigureDogDropdown(fighter1DogDropdown, fighterOptions, selectableFighterDogs, selectedFighter1, SetFighter1FromDropdown);
        ConfigureDogDropdown(fighter2DogDropdown, fighterOptions, selectableFighterDogs, selectedFighter2, SetFighter2FromDropdown);

        ConfigureDogDropdown(parent1DogDropdown, parent1Options, selectableParent1Dogs, selectedParent1, SetParent1FromDropdown);
        ConfigureDogDropdown(parent2DogDropdown, parent2Options, selectableParent2Dogs, selectedParent2, SetParent2FromDropdown);

        isRefreshingDogDropdowns = false;

        UpdateSelectedFightersText();
        UpdateMatchupPreview();
    }

    void BuildCompatibleParentDogLists()
    {
        selectableParent1Dogs.Clear();
        selectableParent2Dogs.Clear();

        AddCompatibleParentDogs(selectableParent1Dogs, selectedParent2);
        AddCompatibleParentDogs(selectableParent2Dogs, selectedParent1);
    }

    void AddCompatibleParentDogs(List<Dog> targetList, Dog selectedOtherParent)
    {
        if (targetList == null)
        {
            return;
        }

        for (int i = 0; i < selectableParentDogs.Count; i++)
        {
            Dog dog = selectableParentDogs[i];

            if (IsCompatibleParentOption(dog, selectedOtherParent))
            {
                targetList.Add(dog);
            }
        }
    }

    bool IsCompatibleParentOption(Dog dog, Dog selectedOtherParent)
    {
        if (dog == null)
        {
            return false;
        }

        if (selectedOtherParent == null)
        {
            return true;
        }

        if (dog == selectedOtherParent)
        {
            return false;
        }

        return dog.gender != selectedOtherParent.gender;
    }

    List<string> BuildDogDropdownOptions(List<Dog> dogs)
    {
        List<string> options = new List<string>();
        options.Add("None");

        foreach (Dog dog in dogs)
        {
            options.Add($"{dog.dogName} ({dog.gender})");
        }

        return options;
    }

    List<string> BuildParentDogDropdownOptions(List<Dog> dogs)
    {
        List<string> options = new List<string>();
        options.Add("None");

        foreach (Dog dog in dogs)
        {
            if (dog == null)
            {
                continue;
            }

            string dogName = string.IsNullOrWhiteSpace(dog.dogName) ? "Unnamed" : dog.dogName.Trim();
            string breedName = GetDisplayBreedNameForDropdown(dog.breed);
            options.Add($"{dogName} — {dog.gender} — {breedName}");
        }

        return options;
    }

    void ClearInvalidSelections()
    {
        if (selectedFighter1 != null && !CanFightDog(selectedFighter1))
        {
            selectedFighter1 = null;
        }

        if (selectedFighter2 != null && !CanFightDog(selectedFighter2))
        {
            selectedFighter2 = null;
        }

        if (selectedParent1 != null && !CanBreedDog(selectedParent1))
        {
            selectedParent1 = null;
        }

        if (selectedParent2 != null && !CanBreedDog(selectedParent2))
        {
            selectedParent2 = null;
        }

        if (selectedParent1 != null && selectedParent2 != null && selectedParent1.gender == selectedParent2.gender)
        {
            selectedParent2 = null;
        }
    }
    void SetParent1FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index, selectableParent1Dogs);
        SelectParent1(dog);
    }

    void SetParent2FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index, selectableParent2Dogs);
        SelectParent2(dog);
    }
    void ConfigureDogDropdown(
        TMP_Dropdown dropdown,
        List<string> options,
        List<Dog> selectableDogs,
        Dog selectedDog,
        UnityAction<int> callback
    )
    {
        if (dropdown == null) return;

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int selectedIndex = GetDropdownIndexForDog(selectedDog, selectableDogs);

        dropdown.SetValueWithoutNotify(selectedIndex);
        dropdown.RefreshShownValue();

        dropdown.interactable = selectableDogs.Count > 0;

        dropdown.onValueChanged.AddListener(callback);
    }

    int GetDropdownIndexForDog(Dog dog, List<Dog> selectableDogs)
    {
        if (dog == null) return 0;

        int dogIndex = selectableDogs.IndexOf(dog);

        if (dogIndex < 0)
        {
            return 0;
        }

        return dogIndex + 1;
    }

    Dog GetDogFromDropdownIndex(int index, List<Dog> selectableDogs)
    {
        if (index <= 0) return null;

        int dogIndex = index - 1;

        if (dogIndex < 0 || dogIndex >= selectableDogs.Count)
        {
            return null;
        }

        return selectableDogs[dogIndex];
    }

    void SetFighter1FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index, selectableFighterDogs);
        SelectFighter1(dog);
    }

    void SetFighter2FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index, selectableFighterDogs);
        SelectFighter2(dog);
    }

    void SetupStrategyDropdowns()
    {
        if (fighter1StrategyDropdown != null)
        {
            fighter1StrategyDropdown.ClearOptions();

            fighter1StrategyDropdown.AddOptions(new List<string>
            {
                "Balanced",
                "Rush Early",
                "Counter Plan",
                "Wear Down",
                "Defensive Shell",
                "All In",
                "Second Wind"
            });

            fighter1StrategyDropdown.value = 0;
            fighter1StrategyDropdown.RefreshShownValue();

            fighter1StrategyDropdown.onValueChanged.RemoveAllListeners();
            fighter1StrategyDropdown.onValueChanged.AddListener(SetFighter1Strategy);
        }
        else
        {
            Debug.LogWarning("Fighter 1 Strategy Dropdown is not assigned.");
        }

        if (fighter2StrategyDropdown != null)
        {
            fighter2StrategyDropdown.ClearOptions();

            fighter2StrategyDropdown.AddOptions(new List<string>
            {
                "Balanced",
                "Rush Early",
                "Counter Plan",
                "Wear Down",
                "Defensive Shell",
                "All In",
                "Second Wind"
            });

            fighter2StrategyDropdown.value = 0;
            fighter2StrategyDropdown.RefreshShownValue();

            fighter2StrategyDropdown.onValueChanged.RemoveAllListeners();
            fighter2StrategyDropdown.onValueChanged.AddListener(SetFighter2Strategy);
        }
        else
        {
            Debug.LogWarning("Fighter 2 Strategy Dropdown is not assigned.");
        }
    }

    void SetFighter1Strategy(int index)
    {
        fighter1Strategy = (FightStrategy)index;
        UpdateMatchupPreview();
    }

    void SetFighter2Strategy(int index)
    {
        fighter2Strategy = (FightStrategy)index;
        UpdateMatchupPreview();
    }

    void UpdateSelectedFightersText()
    {
        if (selectedFightersText == null) return;

        string fighter1Name = selectedFighter1 != null ? selectedFighter1.dogName : "None";
        string fighter2Name = selectedFighter2 != null ? selectedFighter2.dogName : "None";

        selectedFightersText.text =
            $"Fighter/Parent 1: {fighter1Name}\n" +
            $"Fighter/Parent 2: {fighter2Name}";
    }

    void UpdateMatchupPreview()
    {
        if (matchupPreviewText == null) return;

        if (selectedFighter1 == null || selectedFighter2 == null)
        {
            matchupPreviewText.text = "Select two dogs to preview matchup.";
            return;
        }

        int score1 = CalculateDogScore(selectedFighter1, selectedFighter2, fighter1Strategy, fighter2Strategy);
        int score2 = CalculateDogScore(selectedFighter2, selectedFighter1, fighter2Strategy, fighter1Strategy);

        int totalScore = Mathf.Max(1, score1 + score2);

        int chance1 = Mathf.RoundToInt((float)score1 / totalScore * 100f);
        int chance2 = 100 - chance1;

        string styleEdge = GetStyleEdgeText(selectedFighter1, selectedFighter2);

        matchupPreviewText.text =
            $"<b>{selectedFighter1.dogName} vs {selectedFighter2.dogName}</b>\n\n" +
            $"{selectedFighter1.dogName} Win Chance: {chance1}%\n" +
            $"{selectedFighter2.dogName} Win Chance: {chance2}%\n\n" +
            $"{selectedFighter1.dogName} Strategy: {fighter1Strategy}\n" +
            $"{selectedFighter2.dogName} Strategy: {fighter2Strategy}\n\n" +
            $"{selectedFighter1.dogName} Traits: {selectedFighter1.GetTraitSummary()}\n" +
            $"{selectedFighter2.dogName} Traits: {selectedFighter2.GetTraitSummary()}\n\n" +
            $"{styleEdge}";
    }

    int CalculateDogScore(Dog dog, Dog opponent, FightStrategy dogStrategy, FightStrategy opponentStrategy)
    {
        int score = 0;

        score += dog.strength * 2;
        score += dog.agility * 2;
        score += dog.stamina * 2;
        score += dog.GetIntelligence();
        score += dog.level * 5;
        score += dog.wins * 3;
        score -= dog.losses * 2;

        switch (dog.fightStyle)
        {
            case FightStyle.Rushdown:
                score += 8;

                if (opponent.fightStyle == FightStyle.Tank)
                {
                    score -= 5;
                }

                break;

            case FightStyle.Counter:
                if (opponent.fightStyle == FightStyle.Rushdown)
                {
                    score += 12;
                }
                else
                {
                    score += 3;
                }

                break;

            case FightStyle.Tank:
                score += dog.stamina / 2;

                if (opponent.fightStyle == FightStyle.Rushdown)
                {
                    score += 6;
                }

                break;

            case FightStyle.Wildcard:
                score += 5;
                break;

            case FightStyle.Balanced:
            default:
                score += 5;
                break;
        }

        switch (dogStrategy)
        {
            case FightStrategy.RushEarly:
                score += 8;
                break;

            case FightStrategy.CounterPlan:
                if (opponentStrategy == FightStrategy.RushEarly || opponentStrategy == FightStrategy.AllIn)
                {
                    score += 12;
                }
                else
                {
                    score += 2;
                }
                break;

            case FightStrategy.WearDown:
                score += dog.stamina / 3;
                break;

            case FightStrategy.DefensiveShell:
                score += 5;
                break;

            case FightStrategy.AllIn:
                score += 6;
                break;

            case FightStrategy.SecondWind:
                score += 4;
                score += dog.agility / 4;
                score += dog.stamina / 5;
                score += dog.GetIntelligence() / 2;

                if (opponentStrategy == FightStrategy.WearDown || opponentStrategy == FightStrategy.CounterPlan)
                {
                    score += 6;
                }
                else if (opponentStrategy == FightStrategy.AllIn)
                {
                    score -= 8;
                }
                else if (opponentStrategy == FightStrategy.RushEarly)
                {
                    score -= 4;
                }

                break;

            case FightStrategy.Balanced:
            default:
                score += 3;
                break;
        }

        score += GetTraitScoreModifier(dog, opponent);

        return Mathf.Max(1, score);
    }

    int GetTraitScoreModifier(Dog dog, Dog opponent)
    {
        int modifier = 0;

        if (dog.HasTrait(DogTrait.Aggressive))
        {
            modifier += 10;
        }

        if (dog.HasTrait(DogTrait.Durable))
        {
            modifier += 12;
        }

        if (dog.HasTrait(DogTrait.GlassCannon))
        {
            modifier += 8;
        }

        if (dog.HasTrait(DogTrait.Clutch))
        {
            modifier += 8;
        }

        if (dog.HasTrait(DogTrait.LateBloomer) && dog.level >= 5)
        {
            modifier += 8;
        }

        if (dog.HasTrait(DogTrait.Prodigy))
        {
            modifier += 4;
        }

        if (opponent.HasTrait(DogTrait.GlassCannon))
        {
            modifier += 4;
        }

        return modifier;
    }

    string GetStyleEdgeText(Dog d1, Dog d2)
    {
        if (d1.fightStyle == FightStyle.Counter && d2.fightStyle == FightStyle.Rushdown)
        {
            return $"Style Edge: {d1.dogName} counters Rushdown.";
        }

        if (d2.fightStyle == FightStyle.Counter && d1.fightStyle == FightStyle.Rushdown)
        {
            return $"Style Edge: {d2.dogName} counters Rushdown.";
        }

        if (d1.fightStyle == FightStyle.Tank && d2.fightStyle == FightStyle.Rushdown)
        {
            return $"Style Edge: {d1.dogName} can absorb early pressure.";
        }

        if (d2.fightStyle == FightStyle.Tank && d1.fightStyle == FightStyle.Rushdown)
        {
            return $"Style Edge: {d2.dogName} can absorb early pressure.";
        }

        if (d1.fightStyle == FightStyle.Wildcard || d2.fightStyle == FightStyle.Wildcard)
        {
            return "Style Edge: Unstable matchup. Wildcard can swing either way.";
        }

        return "Style Edge: No major style advantage.";
    }

    public void AddOwnedDog(Dog newDog)
    {
        if (newDog == null)
        {
            Debug.LogError("Tried to add a null dog.");
            return;
        }

        EnsureDogHasStableId(newDog);
        newDog.NormalizeLegacyStats();
        ownedDogs.Add(newDog);

        DisplayDogs();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();
        RefreshDogSelectionDropdowns();
        SaveStable();

        Debug.Log($"New dog added to stable: {newDog.dogName}");
    }

    public void OpenDogDetail(Dog dog)
    {
        if (dog == null)
        {
            return;
        }

        DogDetailPanelUI panel = GetOrCreateDogDetailPanel();

        if (panel == null)
        {
            Debug.LogWarning("Could not open dog detail panel because no Canvas was found.");
            return;
        }

        panel.Show(dog, this);
    }

    public Dog FindOwnedDogById(string dogId)
    {
        if (string.IsNullOrWhiteSpace(dogId) || ownedDogs == null)
        {
            return null;
        }

        for (int i = 0; i < ownedDogs.Count; i++)
        {
            Dog dog = ownedDogs[i];

            if (dog != null && string.Equals(dog.dogId, dogId, System.StringComparison.Ordinal))
            {
                return dog;
            }
        }

        return null;
    }

    public void EnsureDogHasStableId(Dog dog)
    {
        if (dog == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(dog.dogId) && dog.dogId != "dog_id")
        {
            return;
        }

        dog.dogId = $"dog_{System.Guid.NewGuid().ToString("N")}";
    }

    public string GetCurrentStrategyTextForDog(Dog dog)
    {
        if (dog == null)
        {
            return "Unknown";
        }

        if (dog == selectedFighter1)
        {
            return fighter1Strategy.ToString();
        }

        if (dog == selectedFighter2)
        {
            return fighter2Strategy.ToString();
        }

        return "Not selected";
    }

    DogDetailPanelUI GetOrCreateDogDetailPanel()
    {
        if (dogDetailPanel != null)
        {
            return dogDetailPanel;
        }

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            return null;
        }

        GameObject panelObject = new GameObject("DogDetailPanel", typeof(RectTransform));
        panelObject.transform.SetParent(canvas.transform, false);
        dogDetailPanel = panelObject.AddComponent<DogDetailPanelUI>();
        panelObject.SetActive(false);

        return dogDetailPanel;
    }

    public bool TryRenameDog(Dog dog, string requestedName, out string cleanedName, out string message)
    {
        cleanedName = dog != null ? dog.dogName : string.Empty;
        message = DogNameValidator.RealDogNameMessage;

        if (dog == null)
        {
            return false;
        }

        if (DogNameValidator.IsSameVisibleName(requestedName, dog.dogName))
        {
            message = string.Empty;
            return true;
        }

        if (!DogNameValidator.TryValidateName(requestedName, out cleanedName, out message))
        {
            return false;
        }

        dog.dogName = cleanedName;
        RefreshDogSelectionDropdowns();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();
        if (dogDetailPanel != null)
        {
            dogDetailPanel.RefreshIfShowing(dog);
        }
        DisplayDogs();
        SaveStable();

        return true;
    }

    public void SaveStable()
    {
        StableSaveData saveData = new StableSaveData();
        saveData.currentWeek = currentWeek;

        foreach (Dog dog in ownedDogs)
        {
            if (dog == null) continue;

            EnsureDogHasStableId(dog);

            DogSaveData dogData = new DogSaveData
            {
                dogId = dog.dogId,
                dogName = dog.dogName,
                breed = dog.breed,
                gender = dog.gender,

                generation = dog.generation,
                parentAId = string.IsNullOrWhiteSpace(dog.parentAId) ? dog.parent1Id : dog.parentAId,
                parentBId = string.IsNullOrWhiteSpace(dog.parentBId) ? dog.parent2Id : dog.parentBId,
                parent1Id = dog.parent1Id,
                parent2Id = dog.parent2Id,
                fatherId = dog.fatherId,
                motherId = dog.motherId,
                parentAName = dog.parentAName,
                parentBName = dog.parentBName,
                parentABreed = dog.parentABreed,
                parentBBreed = dog.parentBBreed,
                parentASex = dog.parentASex,
                parentBSex = dog.parentBSex,
                lastBredWeek = dog.lastBredWeek,

                bloodlineName = dog.bloodlineName,
                ancestorStrengthBonus = dog.ancestorStrengthBonus,
                ancestorAgilityBonus = dog.ancestorAgilityBonus,
                ancestorStaminaBonus = dog.ancestorStaminaBonus,
                ancestorBonusSummary = dog.ancestorBonusSummary,
                isBloodlineCarrier = dog.isBloodlineCarrier,

                age = dog.age,
                isDead = dog.isDead,
                isRetired = dog.isRetired,

                strength = dog.strength,
                agility = dog.agility,
                stamina = dog.stamina,
                intelligence = dog.GetIntelligence(),

                strengthPotential = dog.strengthPotential,
                agilityPotential = dog.agilityPotential,
                staminaPotential = dog.staminaPotential,
                intelligencePotential = dog.GetIntelligencePotential(),

                growthRate = dog.growthRate,
                fightStyle = dog.fightStyle,
                primaryTrait = dog.primaryTrait,
                secondaryTrait = dog.secondaryTrait,

                level = dog.level,
                xp = dog.xp,
                xpToNextLevel = dog.xpToNextLevel,

                wins = dog.wins,
                losses = dog.losses,
                totalFights = dog.totalFights
            };

            saveData.dogs.Add(dogData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Stable saved.");
    }

    public void LoadStable()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("No stable save found.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        StableSaveData saveData = JsonUtility.FromJson<StableSaveData>(json);

        if (saveData == null || saveData.dogs == null)
        {
            Debug.LogWarning("Stable save data was invalid.");
            return;
        }

        if (saveData.currentWeek > 0)
        {
            currentWeek = saveData.currentWeek;
        }

        ownedDogs.Clear();

        foreach (DogSaveData savedDog in saveData.dogs)
        {
            Dog dog = ownedDogs.Find(d => d != null && d.dogId == savedDog.dogId);

            if (dog == null)
            {
                dog = ScriptableObject.CreateInstance<Dog>();
                ownedDogs.Add(dog);
            }

            ApplySaveDataToDog(dog, savedDog);
        }

        Debug.Log("Stable loaded.");
    }

    void ApplySaveDataToDog(Dog dog, DogSaveData savedDog)
    {
        dog.dogId = savedDog.dogId;
        EnsureDogHasStableId(dog);
        dog.dogName = savedDog.dogName;
        dog.breed = savedDog.breed;
        dog.gender = GetSavedGenderWithFallback(savedDog);

        dog.generation = savedDog.generation;
        dog.parentAId = savedDog.parentAId;
        dog.parentBId = savedDog.parentBId;
        dog.parent1Id = savedDog.parent1Id;
        dog.parent2Id = savedDog.parent2Id;
        if (string.IsNullOrWhiteSpace(dog.parentAId))
        {
            dog.parentAId = dog.parent1Id;
        }

        if (string.IsNullOrWhiteSpace(dog.parentBId))
        {
            dog.parentBId = dog.parent2Id;
        }

        if (string.IsNullOrWhiteSpace(dog.parent1Id))
        {
            dog.parent1Id = dog.parentAId;
        }

        if (string.IsNullOrWhiteSpace(dog.parent2Id))
        {
            dog.parent2Id = dog.parentBId;
        }
        dog.fatherId = savedDog.fatherId;
        dog.motherId = savedDog.motherId;
        dog.parentAName = savedDog.parentAName;
        dog.parentBName = savedDog.parentBName;
        dog.parentABreed = savedDog.parentABreed;
        dog.parentBBreed = savedDog.parentBBreed;
        dog.parentASex = savedDog.parentASex;
        dog.parentBSex = savedDog.parentBSex;
        dog.lastBredWeek = savedDog.lastBredWeek;

        dog.bloodlineName = savedDog.bloodlineName;
        dog.ancestorStrengthBonus = savedDog.ancestorStrengthBonus;
        dog.ancestorAgilityBonus = savedDog.ancestorAgilityBonus;
        dog.ancestorStaminaBonus = savedDog.ancestorStaminaBonus;
        dog.ancestorBonusSummary = savedDog.ancestorBonusSummary;
        dog.isBloodlineCarrier = savedDog.isBloodlineCarrier;

        dog.age = savedDog.age;
        dog.isDead = savedDog.isDead;
        dog.isRetired = savedDog.isRetired;

        dog.strength = savedDog.strength;
        dog.agility = savedDog.agility;
        dog.stamina = savedDog.stamina;
        dog.intelligence = savedDog.intelligence <= 0 ? Dog.DefaultIntelligence : savedDog.intelligence;

        dog.strengthPotential = savedDog.strengthPotential;
        dog.agilityPotential = savedDog.agilityPotential;
        dog.staminaPotential = savedDog.staminaPotential;
        dog.intelligencePotential = savedDog.intelligencePotential <= 0
            ? Dog.DefaultIntelligencePotential
            : savedDog.intelligencePotential;
        dog.NormalizeLegacyStats();

        dog.growthRate = savedDog.growthRate;
        dog.fightStyle = savedDog.fightStyle;
        dog.primaryTrait = savedDog.primaryTrait;
        dog.secondaryTrait = savedDog.secondaryTrait;

        dog.level = savedDog.level;
        dog.xp = savedDog.xp;
        dog.xpToNextLevel = savedDog.xpToNextLevel;

        dog.wins = savedDog.wins;
        dog.losses = savedDog.losses;
        dog.totalFights = savedDog.totalFights;
    }

    DogGender GetSavedGenderWithFallback(DogSaveData savedDog)
    {
        bool isLuna = savedDog.dogId == "luna" || savedDog.dogName == "Luna";

        if (isLuna && savedDog.gender == DogGender.Male && savedDog.lastBredWeek == 0)
        {
            return DogGender.Female;
        }

        return savedDog.gender;
    }

    public void ClearStableSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(StarterStableSeedKey);
        PlayerPrefs.Save();

        selectedFighter1 = null;
        selectedFighter2 = null;
        selectedParent1 = null;
        selectedParent2 = null;

        fighter1Strategy = FightStrategy.Balanced;
        fighter2Strategy = FightStrategy.Balanced;
        currentWeek = 1;

        LoadStartingDogs();
        TopUpStarterStable();
        SaveStable();
        MarkStarterStableSeeded();

        SetupStrategyDropdowns();
        RefreshDogSelectionDropdowns();

        DisplayDogs();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();

        Debug.Log("Stable save cleared and stable reset.");
    }

    public void AdvanceWeek()
    {
        bool paidUpkeep = false;
        int upkeepCost = 0;

        if (economyManager != null)
        {
            upkeepCost = economyManager.CalculateWeeklyUpkeep(ownedDogs);
            paidUpkeep = economyManager.PayWeeklyUpkeep(ownedDogs);
        }
        else
        {
            Debug.LogWarning("EconomyManager is missing. Advanced week without paying upkeep.");
        }

        currentWeek++;
        RefreshDogSelectionDropdowns();
        DisplayDogs();
        SaveStable();
        Debug.Log($"Advanced to Week {currentWeek}.");

        if (economyManager != null)
        {
            string upkeepText = paidUpkeep
                ? $"Paid {upkeepCost} credits in upkeep."
                : "Could not afford upkeep. Credits dropped to 0.";

            SetNarration($"Week {currentWeek} begins. {upkeepText}");
        }
        else
        {
            SetNarration($"Week {currentWeek} begins.");
        }
    }

    void SetNarration(string message)
    {
        if (narratorManager != null)
        {
            narratorManager.SetNarration(message);
        }
    }
}

[System.Serializable]
public class StableSaveData
{
    public int currentWeek = 1;
    public List<DogSaveData> dogs = new List<DogSaveData>();
}

[System.Serializable]
public class DogSaveData
{
    public string dogId;
    public string dogName;
    public string breed;
    public DogGender gender;

    public int generation;
    public string parentAId;
    public string parentBId;
    public string parent1Id;
    public string parent2Id;
    public string fatherId;
    public string motherId;
    public string parentAName;
    public string parentBName;
    public string parentABreed;
    public string parentBBreed;
    public string parentASex;
    public string parentBSex;
    public int lastBredWeek = -999;

    public string bloodlineName;
    public int ancestorStrengthBonus;
    public int ancestorAgilityBonus;
    public int ancestorStaminaBonus;
    public string ancestorBonusSummary;
    public bool isBloodlineCarrier;

    public int age;
    public bool isDead;
    public bool isRetired;

    public int strength;
    public int agility;
    public int stamina;
    public int intelligence;

    public int strengthPotential;
    public int agilityPotential;
    public int staminaPotential;
    public int intelligencePotential;

    public float growthRate;

    public FightStyle fightStyle;
    public DogTrait primaryTrait;
    public DogTrait secondaryTrait;

    public int level;
    public int xp;
    public int xpToNextLevel;

    public int wins;
    public int losses;
    public int totalFights;
}
