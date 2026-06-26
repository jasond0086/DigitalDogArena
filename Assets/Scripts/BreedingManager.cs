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

    const int MaxTotalAncestorBonus = 5;
    const int MaxSingleStatAncestorBonus = 3;
    const float ParentBloodlineChance = 0.30f;
    const float GrandparentBloodlineChance = 0.18f;
    const float GreatGrandparentBloodlineChance = 0.08f;
    const float DormantCarrierChance = 0.20f;

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

        string headline = $"Newborn Created: {newborn.dogName} • {newborn.gender} • Gen {newborn.generation} • {newborn.breed}";
        string details =
            "NEWBORN BIRTH REPORT\n\n" +
            $"Name: {newborn.dogName}\n" +
            $"Breed: {newborn.breed}\n" +
            $"Gender: {newborn.gender}\n" +
            $"Generation: {newborn.generation}\n\n" +
            "STATS\n" +
            $"Strength: {newborn.strength} / {newborn.strengthPotential} potential\n" +
            $"Agility: {newborn.agility} / {newborn.agilityPotential} potential\n" +
            $"Stamina: {newborn.stamina} / {newborn.staminaPotential} potential\n\n" +
            $"Potential: {newborn.GetPotentialTitle()}\n" +
            $"Growth: x{newborn.growthRate:F2}\n" +
            $"Fight Style: {newborn.fightStyle}\n" +
            $"Traits: {newborn.GetTraitSummary()}\n\n" +
            "BLOODLINE\n" +
            $"Parents: {parent1.dogName} ({parent1.gender}) x {parent2.dogName} ({parent2.gender})\n" +
            $"{newborn.ancestorBonusSummary}\n" +
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
        newborn.fatherId = parent1.gender == DogGender.Male ? parent1.dogId : parent2.dogId;
        newborn.motherId = parent1.gender == DogGender.Female ? parent1.dogId : parent2.dogId;
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
        ApplyAncestorBloodlineBonus(parent1, parent2, newborn);

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

    void ApplyAncestorBloodlineBonus(Dog parent1, Dog parent2, Dog newborn)
    {
        if (newborn == null)
        {
            return;
        }

        int totalBonus = 0;
        int strengthBonus = 0;
        int agilityBonus = 0;
        int staminaBonus = 0;
        string bloodlineName = "";
        System.Collections.Generic.List<string> rollLog = new System.Collections.Generic.List<string>();

        System.Collections.Generic.List<BloodlineCandidate> candidates =
            new System.Collections.Generic.List<BloodlineCandidate>();

        AddBloodlineCandidate(candidates, parent1, ParentBloodlineChance, "parent");
        AddBloodlineCandidate(candidates, parent2, ParentBloodlineChance, "parent");

        Dog parent1Father = FindAncestor(parent1.fatherId, parent1.parent1Id);
        Dog parent1Mother = FindAncestor(parent1.motherId, parent1.parent2Id);
        Dog parent2Father = FindAncestor(parent2.fatherId, parent2.parent1Id);
        Dog parent2Mother = FindAncestor(parent2.motherId, parent2.parent2Id);

        AddBloodlineCandidate(candidates, parent1Father, GrandparentBloodlineChance, "grandparent");
        AddBloodlineCandidate(candidates, parent1Mother, GrandparentBloodlineChance, "grandparent");
        AddBloodlineCandidate(candidates, parent2Father, GrandparentBloodlineChance, "grandparent");
        AddBloodlineCandidate(candidates, parent2Mother, GrandparentBloodlineChance, "grandparent");

        AddGreatGrandparentCandidates(candidates, parent1Father);
        AddGreatGrandparentCandidates(candidates, parent1Mother);
        AddGreatGrandparentCandidates(candidates, parent2Father);
        AddGreatGrandparentCandidates(candidates, parent2Mother);

        foreach (BloodlineCandidate candidate in candidates)
        {
            if (candidate.dog == null || totalBonus >= MaxTotalAncestorBonus)
            {
                continue;
            }

            float roll = Random.value;
            bool triggered = roll < candidate.chance;
            rollLog.Add($"{candidate.label} {candidate.dog.dogName}: {roll:0.00}/{candidate.chance:0.00}");

            if (!triggered)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(bloodlineName))
            {
                bloodlineName = GetBloodlineName(candidate.dog);
            }

            int bonusToApply = Mathf.Min(Random.Range(1, 4), MaxTotalAncestorBonus - totalBonus);
            ApplyRandomAncestorStatBonus(
                bonusToApply,
                ref strengthBonus,
                ref agilityBonus,
                ref staminaBonus,
                ref totalBonus);
        }

        if (totalBonus > 0)
        {
            newborn.bloodlineName = bloodlineName;
            newborn.isBloodlineCarrier = true;

            int originalStrengthPotential = newborn.strengthPotential;
            int originalAgilityPotential = newborn.agilityPotential;
            int originalStaminaPotential = newborn.staminaPotential;

            newborn.strengthPotential = Mathf.Clamp(originalStrengthPotential + strengthBonus, 1, 120);
            newborn.agilityPotential = Mathf.Clamp(originalAgilityPotential + agilityBonus, 1, 120);
            newborn.staminaPotential = Mathf.Clamp(originalStaminaPotential + staminaBonus, 1, 120);

            newborn.ancestorStrengthBonus = newborn.strengthPotential - originalStrengthPotential;
            newborn.ancestorAgilityBonus = newborn.agilityPotential - originalAgilityPotential;
            newborn.ancestorStaminaBonus = newborn.staminaPotential - originalStaminaPotential;

            string appliedBonusSummary = FormatAncestorBonus(
                newborn.ancestorStrengthBonus,
                newborn.ancestorAgilityBonus,
                newborn.ancestorStaminaBonus);

            if (string.IsNullOrWhiteSpace(appliedBonusSummary))
            {
                appliedBonusSummary = "no potential room after cap";
            }

            newborn.ancestorBonusSummary =
                $"Ancestor bonus triggered: {bloodlineName} {appliedBonusSummary}";

            LogAncestorRolls(newborn, rollLog);
            return;
        }

        Dog carrierSource = GetBloodlineCarrierSource(parent1, parent2);

        if (carrierSource != null && Random.value < DormantCarrierChance)
        {
            newborn.bloodlineName = GetBloodlineName(carrierSource);
            newborn.isBloodlineCarrier = true;
            newborn.ancestorBonusSummary = $"Dormant carrier: carries {newborn.bloodlineName}";
            LogAncestorRolls(newborn, rollLog);
            return;
        }

        newborn.ancestorBonusSummary = "No ancestor bonus triggered";

        if (rollLog.Count > 0)
        {
            newborn.ancestorBonusSummary += $" ({string.Join(", ", rollLog)})";
        }

        LogAncestorRolls(newborn, rollLog);
    }

    void AddBloodlineCandidate(
        System.Collections.Generic.List<BloodlineCandidate> candidates,
        Dog dog,
        float chance,
        string label)
    {
        if (dog == null || candidates == null)
        {
            return;
        }

        candidates.Add(new BloodlineCandidate
        {
            dog = dog,
            chance = chance,
            label = label
        });
    }

    void AddGreatGrandparentCandidates(System.Collections.Generic.List<BloodlineCandidate> candidates, Dog grandparent)
    {
        if (grandparent == null)
        {
            return;
        }

        AddBloodlineCandidate(candidates, FindAncestor(grandparent.fatherId, grandparent.parent1Id), GreatGrandparentBloodlineChance, "great-grandparent");
        AddBloodlineCandidate(candidates, FindAncestor(grandparent.motherId, grandparent.parent2Id), GreatGrandparentBloodlineChance, "great-grandparent");
    }

    Dog FindAncestor(string preferredId, string fallbackId)
    {
        Dog ancestor = FindDogById(preferredId);

        if (ancestor != null)
        {
            return ancestor;
        }

        return FindDogById(fallbackId);
    }

    Dog FindDogById(string dogId)
    {
        if (dogManager == null ||
            dogManager.ownedDogs == null ||
            string.IsNullOrWhiteSpace(dogId))
        {
            return null;
        }

        return dogManager.ownedDogs.Find(dog => dog != null && dog.dogId == dogId);
    }

    void ApplyRandomAncestorStatBonus(
        int bonusToApply,
        ref int strengthBonus,
        ref int agilityBonus,
        ref int staminaBonus,
        ref int totalBonus)
    {
        int safety = 0;

        while (bonusToApply > 0 &&
               totalBonus < MaxTotalAncestorBonus &&
               safety < 30)
        {
            safety++;
            int statIndex = Random.Range(0, 3);

            if (statIndex == 0 && strengthBonus < MaxSingleStatAncestorBonus)
            {
                strengthBonus++;
                totalBonus++;
                bonusToApply--;
            }
            else if (statIndex == 1 && agilityBonus < MaxSingleStatAncestorBonus)
            {
                agilityBonus++;
                totalBonus++;
                bonusToApply--;
            }
            else if (statIndex == 2 && staminaBonus < MaxSingleStatAncestorBonus)
            {
                staminaBonus++;
                totalBonus++;
                bonusToApply--;
            }
        }
    }

    string GetBloodlineName(Dog dog)
    {
        if (dog != null && !string.IsNullOrWhiteSpace(dog.bloodlineName))
        {
            return dog.bloodlineName;
        }

        string[] bloodlineNames =
        {
            "Ironjaw Blood",
            "Apex Line",
            "Street King Blood",
            "Prodigy Spark",
            "Old Yard Stock"
        };

        if (dog != null && dog.HasTrait(DogTrait.Prodigy))
        {
            return "Prodigy Spark";
        }

        if (dog != null && dog.GetPotentialTier() == "Apex")
        {
            return "Apex Line";
        }

        if (dog != null && dog.strengthPotential >= dog.agilityPotential &&
            dog.strengthPotential >= dog.staminaPotential)
        {
            return "Ironjaw Blood";
        }

        return bloodlineNames[Random.Range(0, bloodlineNames.Length)];
    }

    Dog GetBloodlineCarrierSource(Dog parent1, Dog parent2)
    {
        if (HasBloodline(parent1))
        {
            return parent1;
        }

        if (HasBloodline(parent2))
        {
            return parent2;
        }

        return null;
    }

    bool HasBloodline(Dog dog)
    {
        return dog != null &&
               (dog.isBloodlineCarrier || !string.IsNullOrWhiteSpace(dog.bloodlineName));
    }

    string FormatAncestorBonus(int strengthBonus, int agilityBonus, int staminaBonus)
    {
        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();

        if (strengthBonus > 0)
        {
            parts.Add($"+{strengthBonus} STR potential");
        }

        if (agilityBonus > 0)
        {
            parts.Add($"+{agilityBonus} AGI potential");
        }

        if (staminaBonus > 0)
        {
            parts.Add($"+{staminaBonus} STA potential");
        }

        return string.Join(", ", parts);
    }

    void LogAncestorRolls(Dog newborn, System.Collections.Generic.List<string> rollLog)
    {
        if (newborn == null || rollLog == null || rollLog.Count == 0)
        {
            return;
        }

        Debug.Log($"Ancestor bloodline rolls for {newborn.dogName}: {string.Join(", ", rollLog)}");
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

    class BloodlineCandidate
    {
        public Dog dog;
        public float chance;
        public string label;
    }
}
