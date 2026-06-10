using UnityEngine;
using TMPro;

public class BreedingManager : MonoBehaviour
{
    [Header("References")]
    public DogManager dogManager;
    public EconomyManager economyManager;
    public NarratorManager narratorManager;
    public TextMeshProUGUI breedingLog;

    [Header("Breeding Settings")]
    [Range(0, 20)] public int statVariance = 8;
    [Range(0, 20)] public int potentialVariance = 6;
    [Range(0f, 0.5f)] public float growthMutationRange = 0.15f;
    [Range(0f, 1f)] public float styleMutationChance = 0.10f;
    [Range(0f, 1f)] public float traitMutationChance = 0.15f;

    void Awake()
    {
        if (dogManager == null)
        {
            dogManager = GetComponent<DogManager>();
        }

        if (economyManager == null)
        {
            economyManager = GetComponent<EconomyManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = GetComponent<NarratorManager>();
        }
    }

    public void BreedSelectedFighters()
    {
        if (dogManager == null)
        {
            BlockBreeding("DogManager is missing!");
            return;
        }

        dogManager.RefreshDogSelectionDropdowns();

        if (dogManager.parent1DogDropdown == null || dogManager.parent2DogDropdown == null)
        {
            BlockBreeding("Parent dropdown is missing.");
            return;
        }

        if (dogManager.parent1DogDropdown.options == null ||
            dogManager.parent2DogDropdown.options == null ||
            dogManager.parent1DogDropdown.options.Count <= 1 ||
            dogManager.parent2DogDropdown.options.Count <= 1)
        {
            BlockBreeding("No eligible parents are available.");
            return;
        }

        if (dogManager.parent1DogDropdown.value < 0 ||
            dogManager.parent1DogDropdown.value >= dogManager.parent1DogDropdown.options.Count ||
            dogManager.parent2DogDropdown.value < 0 ||
            dogManager.parent2DogDropdown.value >= dogManager.parent2DogDropdown.options.Count)
        {
            BlockBreeding("Invalid parent selection.");
            return;
        }

        Dog parent1 = dogManager.selectedParent1;
        Dog parent2 = dogManager.selectedParent2;

        if (parent1 == null || parent2 == null)
        {
            BlockBreeding("Select two parents first.");
            return;
        }

        if (parent1 == parent2)
        {
            BlockBreeding("A dog cannot breed with itself.");
            return;
        }

        if (parent1.isDead || parent2.isDead)
        {
            BlockBreeding("Deceased dogs cannot breed.");
            return;
        }

        if (parent1.isRetired || parent2.isRetired)
        {
            BlockBreeding("Retired dogs cannot breed for now.");
            return;
        }

        if (parent1.gender == parent2.gender)
        {
            BlockBreeding("Breeding requires one female and one male.");
            return;
        }

        if (!parent1.CanBreedInWeek(dogManager.currentWeek) || !parent2.CanBreedInWeek(dogManager.currentWeek))
        {
            BlockBreeding("One or both dogs have already bred this week.");
            return;
        }

        int breedingCost = CalculateBreedingCost(parent1, parent2);

        if (economyManager == null)
        {
            BlockBreeding("EconomyManager is missing!");
            return;
        }

        if (!economyManager.CanAfford(breedingCost))
        {
            BlockBreeding($"Breeding costs {breedingCost} credits. Not enough credits.");
            return;
        }

        economyManager.SpendCredits(breedingCost, $"Breeding {parent1.dogName} and {parent2.dogName}");

        Dog newborn = CreateNewborn(parent1, parent2);

        if (newborn == null)
        {
            BlockBreeding("Puppy creation failed.");
            return;
        }

        parent1.lastBredWeek = dogManager.currentWeek;
        parent2.lastBredWeek = dogManager.currentWeek;

        dogManager.AddOwnedDog(newborn);

        if (!dogManager.ownedDogs.Contains(newborn))
        {
            BlockBreeding("Dog manager add failed.");
            return;
        }

        dogManager.SaveStable();

        string headline = $"Newborn created: {newborn.dogName} ({newborn.gender}) - {newborn.GetPotentialTitle()}";
        string details =
            "Newborn Created!\n\n" +
            $"Name: {newborn.dogName}\n" +
            $"Breed: {newborn.breed}\n" +
            $"Gender: {newborn.gender}\n" +
            $"Generation: {newborn.generation}\n\n" +
            $"STR: {newborn.strength}\n" +
            $"AGI: {newborn.agility}\n" +
            $"STA: {newborn.stamina}\n" +
            $"Potential: {newborn.GetPotentialTitle()}\n" +
            $"Growth: x{newborn.growthRate:F2}\n" +
            $"Style: {newborn.fightStyle}\n\n" +
            $"Traits: {newborn.GetTraitSummary()}\n\n" +
            $"Parents: {parent1.dogName} ({parent1.gender}) x {parent2.dogName} ({parent2.gender})\n" +
            $"Week: {dogManager.currentWeek}\n" +
            $"Breeding Cost: {breedingCost} credits";

        if (narratorManager != null)
        {
            narratorManager.SetNarration(headline, details);
            Debug.Log(details);
        }
        else
        {
            LogBreedingMessage(details);
        }
    }

    public int CalculateBreedingCost(Dog parent1, Dog parent2)
    {
        if (parent1 == null || parent2 == null)
        {
            return 0;
        }

        return 100 +
               GetTierCost(parent1.GetPotentialTier()) +
               GetTierCost(parent2.GetPotentialTier()) +
               GetTraitPremium(parent1.primaryTrait) +
               GetTraitPremium(parent1.secondaryTrait) +
               GetTraitPremium(parent2.primaryTrait) +
               GetTraitPremium(parent2.secondaryTrait);
    }

    int GetTierCost(string tier)
    {
        switch (tier)
        {
            case "Street": return 25;
            case "Prospect": return 50;
            case "Contender": return 100;
            case "Elite": return 200;
            case "Apex": return 400;
            case "Legendary": return 800;
            default: return 0;
        }
    }

    int GetTraitPremium(DogTrait trait)
    {
        switch (trait)
        {
            case DogTrait.Prodigy: return 150;
            case DogTrait.LateBloomer: return 100;
            case DogTrait.Clutch: return 75;
            case DogTrait.Durable: return 75;
            case DogTrait.Aggressive: return 50;
            case DogTrait.GlassCannon: return 50;
            case DogTrait.None:
            default:
                return 0;
        }
    }

    Dog CreateNewborn(Dog parent1, Dog parent2)
    {
        Dog newborn = ScriptableObject.CreateInstance<Dog>();

        newborn.dogId = System.Guid.NewGuid().ToString();

        newborn.dogName = GenerateName();
        newborn.breed = GenerateBreed(parent1, parent2);
        newborn.gender = GetRandomGender();
        newborn.lastBredWeek = -999;

        newborn.parent1Id = parent1.dogId;
        newborn.parent2Id = parent2.dogId;
        newborn.generation = Mathf.Max(parent1.generation, parent2.generation) + 1;

        newborn.strengthPotential = BreedPotential(parent1.strengthPotential, parent2.strengthPotential);
        newborn.agilityPotential = BreedPotential(parent1.agilityPotential, parent2.agilityPotential);
        newborn.staminaPotential = BreedPotential(parent1.staminaPotential, parent2.staminaPotential);

        newborn.strength = BreedStartingStat(parent1.strength, parent2.strength, newborn.strengthPotential);
        newborn.agility = BreedStartingStat(parent1.agility, parent2.agility, newborn.agilityPotential);
        newborn.stamina = BreedStartingStat(parent1.stamina, parent2.stamina, newborn.staminaPotential);

        newborn.growthRate = BreedGrowthRate(parent1.growthRate, parent2.growthRate);

        newborn.fightStyle = BreedFightStyle(parent1.fightStyle, parent2.fightStyle);
        BreedTraits(parent1, parent2, newborn);
        ApplyTraitGrowthBonus(newborn);

        newborn.level = 1;
        newborn.xp = 0;
        newborn.xpToNextLevel = 100;

        newborn.wins = 0;
        newborn.losses = 0;
        newborn.totalFights = 0;

        return newborn;
    }

    string GetBreedingBlockedMessage(Dog parent1, Dog parent2)
    {
        if (parent1.isDead || parent2.isDead)
        {
            return "Deceased dogs cannot breed.";
        }

        if (parent1.isRetired || parent2.isRetired)
        {
            return "Retired dogs cannot breed for now.";
        }

        if (!parent1.CanBreedInWeek(dogManager.currentWeek))
        {
            return $"{parent1.dogName} has already bred this week.";
        }

        if (!parent2.CanBreedInWeek(dogManager.currentWeek))
        {
            return $"{parent2.dogName} has already bred this week.";
        }

        return "These dogs cannot breed right now.";
    }

    DogGender GetRandomGender()
    {
        if (Random.value < 0.5f)
        {
            return DogGender.Male;
        }

        return DogGender.Female;
    }

    int BreedPotential(int parentPotential1, int parentPotential2)
    {
        int bestParentPotential = Mathf.Max(parentPotential1, parentPotential2);
        int average = Mathf.RoundToInt((parentPotential1 + parentPotential2) / 2f);
        int inheritanceFloor = Mathf.RoundToInt(bestParentPotential * 0.85f);
        int inheritedPotential = Mathf.Max(average, inheritanceFloor);
        int mutation = Random.Range(-potentialVariance, potentialVariance + 1);

        return Mathf.Clamp(inheritedPotential + mutation, 40, 120);
    }

    int BreedStartingStat(int parentStat1, int parentStat2, int potential)
    {
        int average = Mathf.RoundToInt((parentStat1 + parentStat2) / 2f);
        int mutation = Random.Range(-statVariance, statVariance + 1);
        int upperLimit = Mathf.Min(100, potential);

        return Mathf.Clamp(average + mutation, 1, upperLimit);
    }

    float BreedGrowthRate(float parentGrowth1, float parentGrowth2)
    {
        float average = (parentGrowth1 + parentGrowth2) / 2f;
        float mutation = Random.Range(-growthMutationRange, growthMutationRange);

        return Mathf.Clamp(average + mutation, 0.5f, 2.0f);
    }

    FightStyle BreedFightStyle(FightStyle parentStyle1, FightStyle parentStyle2)
    {
        float mutationRoll = Random.value;

        if (mutationRoll < styleMutationChance)
        {
            return GetRandomFightStyle();
        }

        if (Random.value < 0.5f)
        {
            return parentStyle1;
        }

        return parentStyle2;
    }

    void BreedTraits(Dog parent1, Dog parent2, Dog newborn)
    {
        newborn.primaryTrait = RollInheritedTrait(parent1, parent2);
        newborn.secondaryTrait = DogTrait.None;

        if (Random.value < 0.35f)
        {
            newborn.secondaryTrait = RollInheritedTrait(parent1, parent2);

            if (newborn.secondaryTrait == newborn.primaryTrait)
            {
                newborn.secondaryTrait = DogTrait.None;
            }
        }
    }

    DogTrait RollInheritedTrait(Dog parent1, Dog parent2)
    {
        if (Random.value < traitMutationChance)
        {
            return GetRandomTrait();
        }

        ListTraitPool traitPool = new ListTraitPool();
        traitPool.Add(parent1.primaryTrait);
        traitPool.Add(parent1.secondaryTrait);
        traitPool.Add(parent2.primaryTrait);
        traitPool.Add(parent2.secondaryTrait);

        return traitPool.GetRandomTrait();
    }

    DogTrait GetRandomTrait()
    {
        int traitCount = System.Enum.GetValues(typeof(DogTrait)).Length;
        int randomIndex = Random.Range(1, traitCount);

        return (DogTrait)randomIndex;
    }

    void ApplyTraitGrowthBonus(Dog dog)
    {
        if (dog.HasTrait(DogTrait.Prodigy))
        {
            dog.growthRate = Mathf.Clamp(dog.growthRate + 0.15f, 0.5f, 2.0f);
        }

        if (dog.HasTrait(DogTrait.LateBloomer))
        {
            dog.growthRate = Mathf.Clamp(dog.growthRate + 0.08f, 0.5f, 2.0f);
            dog.strengthPotential = Mathf.Clamp(dog.strengthPotential + 3, 1, 120);
            dog.agilityPotential = Mathf.Clamp(dog.agilityPotential + 3, 1, 120);
            dog.staminaPotential = Mathf.Clamp(dog.staminaPotential + 3, 1, 120);
        }
    }

    FightStyle GetRandomFightStyle()
    {
        int styleCount = System.Enum.GetValues(typeof(FightStyle)).Length;
        int randomIndex = Random.Range(0, styleCount);

        return (FightStyle)randomIndex;
    }

    string GenerateBreed(Dog parent1, Dog parent2)
    {
        if (parent1.breed == parent2.breed)
        {
            return parent1.breed;
        }

        return $"{parent1.breed} / {parent2.breed}";
    }

    string GenerateName()
    {
        string[] prefixes =
        {
            "Neo",
            "Vex",
            "Rex",
            "Kairo",
            "Zoro",
            "Luna",
            "Nyx",
            "Titan",
            "Ash",
            "Onyx",
            "Rogue",
            "Fang"
        };

        string[] suffixes =
        {
            "fang",
            "claw",
            "storm",
            "shade",
            "strike",
            "maw",
            "blood",
            "ghost",
            "bane",
            "howl",
            "rift",
            "volt"
        };

        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];

        return prefix + suffix;
    }

    void BlockBreeding(string message)
    {
        LogBreedingMessage(message);
    }

    private void LogBreedingMessage(string message)
    {
        if (narratorManager == null)
        {
            Debug.LogWarning("Breeding narratorManager is null.");
            narratorManager = GetComponent<NarratorManager>();
        }

        if (narratorManager == null)
        {
            narratorManager = FindFirstObjectByType<NarratorManager>();
        }

        if (narratorManager != null)
        {
            Debug.Log("Sending breeding message to NarratorManager.");
            narratorManager.SetNarration(message);
        }
        else if (breedingLog != null)
        {
            breedingLog.text = message;
        }

        Debug.Log(message);
    }

    class ListTraitPool
    {
        private readonly System.Collections.Generic.List<DogTrait> traits =
            new System.Collections.Generic.List<DogTrait>();

        public void Add(DogTrait trait)
        {
            if (trait != DogTrait.None)
            {
                traits.Add(trait);
            }
        }

        public DogTrait GetRandomTrait()
        {
            if (traits.Count == 0)
            {
                return DogTrait.None;
            }

            return traits[Random.Range(0, traits.Count)];
        }
    }
}
