using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class DogManager : MonoBehaviour
{
    private const string SaveKey = "DOG_STABLE_SAVE";

    [Header("Owned Dogs")]
    public List<Dog> ownedDogs = new List<Dog>();

    [Header("UI")]
    public Transform dogContainer;
    public GameObject dogCardPrefab;
    public TextMeshProUGUI selectedFightersText;
    public TextMeshProUGUI matchupPreviewText;

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

    private List<Dog> selectableDogs = new List<Dog>();
    private bool isRefreshingDogDropdowns = false;

    void Start()
    {
        LoadStartingDogs();
        LoadStable();

        SetupStrategyDropdowns();
        RefreshDogSelectionDropdowns();

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

        ownedDogs.Add(dogInstance);
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

    public void SelectParent1(Dog dog)
    {
        if (dog == null)
        {
            selectedParent1 = null;
            RefreshDogSelectionDropdowns();
            return;
        }

        if (!CanUseDog(dog))
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

        if (!CanUseDog(dog))
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

        if (!CanUseDog(dog))
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

        if (!CanUseDog(dog))
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

    bool CanUseDog(Dog dog)
    {
        if (dog == null) return false;
        if (dog.isDead) return false;
        if (dog.isRetired) return false;

        return true;
    }

    void RefreshDogSelectionDropdowns()
    {
        isRefreshingDogDropdowns = true;

        selectableDogs.Clear();

        foreach (Dog dog in ownedDogs)
        {
            if (CanUseDog(dog))
            {
                selectableDogs.Add(dog);
            }
        }

        List<string> options = new List<string>();
        options.Add("None");

        foreach (Dog dog in selectableDogs)
        {
            options.Add(dog.dogName);
        }

        ConfigureDogDropdown(fighter1DogDropdown, options, selectedFighter1, SetFighter1FromDropdown);
        ConfigureDogDropdown(fighter2DogDropdown, options, selectedFighter2, SetFighter2FromDropdown);

        ConfigureDogDropdown(parent1DogDropdown, options, selectedParent1, SetParent1FromDropdown);
        ConfigureDogDropdown(parent2DogDropdown, options, selectedParent2, SetParent2FromDropdown);

        isRefreshingDogDropdowns = false;
    }
    void SetParent1FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index);
        SelectParent1(dog);
    }

    void SetParent2FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index);
        SelectParent2(dog);
    }
    void ConfigureDogDropdown(
        TMP_Dropdown dropdown,
        List<string> options,
        Dog selectedDog,
        UnityAction<int> callback
    )
    {
        if (dropdown == null) return;

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int selectedIndex = GetDropdownIndexForDog(selectedDog);

        dropdown.SetValueWithoutNotify(selectedIndex);
        dropdown.RefreshShownValue();

        dropdown.interactable = selectableDogs.Count > 0;

        dropdown.onValueChanged.AddListener(callback);
    }

    int GetDropdownIndexForDog(Dog dog)
    {
        if (dog == null) return 0;

        int dogIndex = selectableDogs.IndexOf(dog);

        if (dogIndex < 0)
        {
            return 0;
        }

        return dogIndex + 1;
    }

    Dog GetDogFromDropdownIndex(int index)
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

        Dog dog = GetDogFromDropdownIndex(index);
        SelectFighter1(dog);
    }

    void SetFighter2FromDropdown(int index)
    {
        if (isRefreshingDogDropdowns) return;

        Dog dog = GetDogFromDropdownIndex(index);
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
                "All In"
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
                "All In"
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

        ownedDogs.Add(newDog);

        DisplayDogs();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();
        RefreshDogSelectionDropdowns();
        SaveStable();

        Debug.Log($"New dog added to stable: {newDog.dogName}");
    }

    public void SaveStable()
    {
        StableSaveData saveData = new StableSaveData();

        foreach (Dog dog in ownedDogs)
        {
            if (dog == null) continue;

            DogSaveData dogData = new DogSaveData
            {
                dogId = dog.dogId,
                dogName = dog.dogName,
                breed = dog.breed,

                generation = dog.generation,
                parent1Id = dog.parent1Id,
                parent2Id = dog.parent2Id,

                age = dog.age,
                isDead = dog.isDead,
                isRetired = dog.isRetired,

                strength = dog.strength,
                agility = dog.agility,
                stamina = dog.stamina,

                strengthPotential = dog.strengthPotential,
                agilityPotential = dog.agilityPotential,
                staminaPotential = dog.staminaPotential,

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
        dog.dogName = savedDog.dogName;
        dog.breed = savedDog.breed;

        dog.generation = savedDog.generation;
        dog.parent1Id = savedDog.parent1Id;
        dog.parent2Id = savedDog.parent2Id;

        dog.age = savedDog.age;
        dog.isDead = savedDog.isDead;
        dog.isRetired = savedDog.isRetired;

        dog.strength = savedDog.strength;
        dog.agility = savedDog.agility;
        dog.stamina = savedDog.stamina;

        dog.strengthPotential = savedDog.strengthPotential;
        dog.agilityPotential = savedDog.agilityPotential;
        dog.staminaPotential = savedDog.staminaPotential;

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

    public void ClearStableSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        selectedFighter1 = null;
        selectedFighter2 = null;
        selectedParent1 = null;
        selectedParent2 = null;

        fighter1Strategy = FightStrategy.Balanced;
        fighter2Strategy = FightStrategy.Balanced;

        LoadStartingDogs();

        SetupStrategyDropdowns();
        RefreshDogSelectionDropdowns();

        DisplayDogs();
        UpdateSelectedFightersText();
        UpdateMatchupPreview();

        Debug.Log("Stable save cleared and stable reset.");
    }
}

[System.Serializable]
public class StableSaveData
{
    public List<DogSaveData> dogs = new List<DogSaveData>();
}

[System.Serializable]
public class DogSaveData
{
    public string dogId;
    public string dogName;
    public string breed;

    public int generation;
    public string parent1Id;
    public string parent2Id;

    public int age;
    public bool isDead;
    public bool isRetired;

    public int strength;
    public int agility;
    public int stamina;

    public int strengthPotential;
    public int agilityPotential;
    public int staminaPotential;

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
