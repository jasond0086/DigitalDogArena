using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class BreedingManager : MonoBehaviour
{
    [Header("References")]
    public DogManager dogManager;
    public EconomyManager economyManager;
    public NarratorManager narratorManager;
    public TextMeshProUGUI breedingLog;

    [Header("Parent Portraits")]
    public Image parent1PortraitImage;
    public Image parent2PortraitImage;
    public Sprite defaultDogPortraitSprite;

    [Header("Parent Preview Text")]
    public TextMeshProUGUI parent1PreviewText;
    public TextMeshProUGUI parent2PreviewText;

    [Header("Puppy Preview")]
    public Image puppyPreviewImage;
    public TextMeshProUGUI puppyPreviewText;
    public bool createMissingPreviewFallbacks = true;

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

    Dog lastDisplayedParent1;
    Dog lastDisplayedParent2;

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

        FindBreedingPreviewReferences();
        ConfigurePortraitImage(parent1PortraitImage);
        ConfigurePortraitImage(parent2PortraitImage);
        ConfigurePortraitImage(puppyPreviewImage);
        ConfigurePuppyPreviewText();
        FindParentPreviewTextReferences();
        ConfigureParentPreviewText(parent1PreviewText);
        ConfigureParentPreviewText(parent2PreviewText);
    }

    void Start()
    {
        RefreshParentPortraits(true);
    }

    void Update()
    {
        RefreshParentPortraits(false);
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

        string breedDetails = BuildBreedDetails(newborn, parent1, parent2);
        string bloodlineDetails = BuildBloodlineDetails(newborn, parent1, parent2);

        string headline = $"Newborn Created: {newborn.dogName} • {newborn.gender} • Gen {newborn.generation} • {newborn.breed}";
        string details =
            "NEWBORN BIRTH REPORT\n\n" +
            $"Name: {newborn.dogName}\n" +
            $"Gender: {newborn.gender}\n" +
            $"Generation: {newborn.generation}\n\n" +
            "BREED\n" +
            breedDetails +
            "STATS\n" +
            $"Strength: {newborn.strength} / {newborn.strengthPotential} potential\n" +
            $"Agility: {newborn.agility} / {newborn.agilityPotential} potential\n" +
            $"Stamina: {newborn.stamina} / {newborn.staminaPotential} potential\n" +
            $"Intelligence: {newborn.GetIntelligence()} / {newborn.GetIntelligencePotential()} potential\n\n" +
            $"Potential: {newborn.GetPotentialTitle()}\n" +
            $"Growth: x{newborn.growthRate:F2}\n" +
            $"Fight Style: {newborn.fightStyle}\n" +
            $"Traits: {newborn.GetTraitSummary()}\n\n" +
            "BLOODLINE\n" +
            bloodlineDetails +
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

    string BuildBloodlineDetails(Dog newborn, Dog parent1, Dog parent2)
    {
        string fatherName = GetParentNameByGender(parent1, parent2, DogGender.Male);
        string motherName = GetParentNameByGender(parent1, parent2, DogGender.Female);
        string bloodlineName = string.IsNullOrWhiteSpace(newborn.bloodlineName)
            ? "None"
            : newborn.bloodlineName;

        return
            $"Puppy: {newborn.dogName}\n" +
            $"Father: {fatherName}\n" +
            $"Mother: {motherName}\n" +
            $"Bloodline: {bloodlineName}\n" +
            $"{BuildAncestorResultLine(newborn)}\n";
    }

    string BuildBreedDetails(Dog newborn, Dog parent1, Dog parent2)
    {
        string fatherBreed = GetParentBreedByGender(parent1, parent2, DogGender.Male);
        string motherBreed = GetParentBreedByGender(parent1, parent2, DogGender.Female);
        bool isHybrid = IsHybridBreed(parent1, parent2);

        return
            $"Breed: {GetDisplayBreedName(newborn.breed)}\n" +
            $"Hybrid: {(isHybrid ? "Yes" : "No")}\n" +
            $"Parents: {fatherBreed} x {motherBreed}\n\n";
    }

    string GetParentNameByGender(Dog parent1, Dog parent2, DogGender gender)
    {
        if (parent1 != null && parent1.gender == gender)
        {
            return parent1.dogName;
        }

        if (parent2 != null && parent2.gender == gender)
        {
            return parent2.dogName;
        }

        return "Unknown";
    }

    string GetParentBreedByGender(Dog parent1, Dog parent2, DogGender gender)
    {
        if (parent1 != null && parent1.gender == gender)
        {
            return GetDisplayBreedName(parent1.breed);
        }

        if (parent2 != null && parent2.gender == gender)
        {
            return GetDisplayBreedName(parent2.breed);
        }

        return "Unknown";
    }

    bool IsHybridBreed(Dog parent1, Dog parent2)
    {
        if (parent1 == null || parent2 == null)
        {
            return false;
        }

        string parent1Breed = parent1.breed == null ? "" : parent1.breed.Trim();
        string parent2Breed = parent2.breed == null ? "" : parent2.breed.Trim();

        return !string.Equals(parent1Breed, parent2Breed, System.StringComparison.OrdinalIgnoreCase);
    }

    string BuildAncestorResultLine(Dog newborn)
    {
        if (newborn == null)
        {
            return "Ancestor Bonus: None triggered";
        }

        bool hasStatBonus =
            newborn.ancestorStrengthBonus > 0 ||
            newborn.ancestorAgilityBonus > 0 ||
            newborn.ancestorStaminaBonus > 0;

        if (hasStatBonus)
        {
            return $"Ancestor Bonus: {newborn.bloodlineName} {FormatAncestorBonusShort(newborn.ancestorStrengthBonus, newborn.ancestorAgilityBonus, newborn.ancestorStaminaBonus)}";
        }

        if (newborn.isBloodlineCarrier && !string.IsNullOrWhiteSpace(newborn.bloodlineName))
        {
            return $"Dormant Bloodline Carrier: {newborn.bloodlineName}";
        }

        return "Ancestor Bonus: None triggered";
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

        EnsureParentLineageId(parent1);
        EnsureParentLineageId(parent2);

        newborn.dogId = System.Guid.NewGuid().ToString();

        newborn.dogName = GenerateName();
        newborn.breed = GenerateBreed(parent1, parent2);
        newborn.gender = GetRandomGender();
        newborn.lastBredWeek = -999;

        newborn.parentAId = parent1.dogId;
        newborn.parentBId = parent2.dogId;
        newborn.parent1Id = parent1.dogId;
        newborn.parent2Id = parent2.dogId;
        newborn.fatherId = parent1.gender == DogGender.Male ? parent1.dogId : parent2.dogId;
        newborn.motherId = parent1.gender == DogGender.Female ? parent1.dogId : parent2.dogId;
        newborn.generation = Mathf.Max(parent1.generation, parent2.generation) + 1;
        StoreParentLineageSnapshot(newborn, parent1, parent2);

        newborn.strengthPotential = BreedPotential(parent1.strengthPotential, parent2.strengthPotential);
        newborn.agilityPotential = BreedPotential(parent1.agilityPotential, parent2.agilityPotential);
        newborn.staminaPotential = BreedPotential(parent1.staminaPotential, parent2.staminaPotential);
        newborn.intelligencePotential = BreedPotential(parent1.GetIntelligencePotential(), parent2.GetIntelligencePotential());

        newborn.strength = BreedStartingStat(parent1.strength, parent2.strength, newborn.strengthPotential);
        newborn.agility = BreedStartingStat(parent1.agility, parent2.agility, newborn.agilityPotential);
        newborn.stamina = BreedStartingStat(parent1.stamina, parent2.stamina, newborn.staminaPotential);
        newborn.intelligence = BreedStartingStat(parent1.GetIntelligence(), parent2.GetIntelligence(), newborn.intelligencePotential);

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

    void EnsureParentLineageId(Dog parent)
    {
        if (parent == null)
        {
            return;
        }

        if (dogManager != null)
        {
            dogManager.EnsureDogHasStableId(parent);
            return;
        }

        if (string.IsNullOrWhiteSpace(parent.dogId) || parent.dogId == "dog_id")
        {
            parent.dogId = System.Guid.NewGuid().ToString();
        }
    }

    void StoreParentLineageSnapshot(Dog newborn, Dog parent1, Dog parent2)
    {
        if (newborn == null)
        {
            return;
        }

        newborn.parentAName = GetLineageName(parent1);
        newborn.parentBName = GetLineageName(parent2);
        newborn.parentABreed = GetLineageBreed(parent1);
        newborn.parentBBreed = GetLineageBreed(parent2);
        newborn.parentASex = GetLineageSex(parent1);
        newborn.parentBSex = GetLineageSex(parent2);
    }

    string GetLineageName(Dog dog)
    {
        return dog == null || string.IsNullOrWhiteSpace(dog.dogName)
            ? "Unknown"
            : dog.dogName.Trim();
    }

    string GetLineageBreed(Dog dog)
    {
        return dog == null || string.IsNullOrWhiteSpace(dog.breed)
            ? "Unknown"
            : GetDisplayBreedName(dog.breed);
    }

    string GetLineageSex(Dog dog)
    {
        return dog == null ? "Unknown" : dog.gender.ToString();
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
            dog.intelligencePotential = Mathf.Clamp(dog.GetIntelligencePotential() + 3, 1, 120);
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

    string FormatAncestorBonusShort(int strengthBonus, int agilityBonus, int staminaBonus)
    {
        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();

        if (strengthBonus > 0)
        {
            parts.Add($"+{strengthBonus} STR Pot");
        }

        if (agilityBonus > 0)
        {
            parts.Add($"+{agilityBonus} AGI Pot");
        }

        if (staminaBonus > 0)
        {
            parts.Add($"+{staminaBonus} STA Pot");
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
        return GetDisplayBreedName(BreedLibrary.GetHybridBreedName(parent1.breed, parent2.breed));
    }

    string GetDisplayBreedName(string breedName)
    {
        string compactBreed = GetCompactBreedName(breedName);

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
                return string.IsNullOrWhiteSpace(breedName) ? "Unknown" : breedName.Trim();
        }
    }

    string GetCompactBreedName(string breedName)
    {
        if (string.IsNullOrWhiteSpace(breedName))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(breedName.Length);

        for (int i = 0; i < breedName.Length; i++)
        {
            char breedCharacter = char.ToLowerInvariant(breedName[i]);

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

    void RefreshParentPortraits(bool forceRefresh)
    {
        if (dogManager == null)
        {
            return;
        }

        bool selectionChanged = forceRefresh;

        if (forceRefresh || dogManager.selectedParent1 != lastDisplayedParent1)
        {
            lastDisplayedParent1 = dogManager.selectedParent1;
            SetParentPortrait(parent1PortraitImage, lastDisplayedParent1);
            SetParentPreviewText(parent1PreviewText, lastDisplayedParent1);
            selectionChanged = true;
        }

        if (forceRefresh || dogManager.selectedParent2 != lastDisplayedParent2)
        {
            lastDisplayedParent2 = dogManager.selectedParent2;
            SetParentPortrait(parent2PortraitImage, lastDisplayedParent2);
            SetParentPreviewText(parent2PreviewText, lastDisplayedParent2);
            selectionChanged = true;
        }

        if (selectionChanged)
        {
            UpdatePuppyPreview(lastDisplayedParent1, lastDisplayedParent2);
        }
    }

    void SetParentPortrait(Image portraitImage, Dog selectedDog)
    {
        if (portraitImage == null)
        {
            return;
        }

        Sprite portraitSprite = defaultDogPortraitSprite;

        if (selectedDog != null)
        {
            portraitSprite = DogPortraitLibrary.ChooseStableCardPortrait(
                selectedDog,
                selectedDog.dogSprite,
                defaultDogPortraitSprite);
        }

        SetPreviewImage(portraitImage, portraitSprite);
    }

    void ConfigurePortraitImage(Image portraitImage)
    {
        if (portraitImage == null)
        {
            return;
        }

        portraitImage.raycastTarget = false;
        portraitImage.preserveAspect = true;
    }

    void FindBreedingPreviewReferences()
    {
        if (parent1PortraitImage == null)
        {
            parent1PortraitImage = FindSceneComponentByName<Image>("Parent1PortraitImage");
        }

        if (parent2PortraitImage == null)
        {
            parent2PortraitImage = FindSceneComponentByName<Image>("Parent2PortraitImage");
        }

        if (puppyPreviewText == null)
        {
            puppyPreviewText = FindSceneComponentByName<TextMeshProUGUI>("PuppyPreviewText");
        }

        if (puppyPreviewText == null)
        {
            puppyPreviewText = FindSceneComponentByName<TextMeshProUGUI>("BreedCenterText");
        }

        if (puppyPreviewImage == null)
        {
            puppyPreviewImage = FindSceneComponentByName<Image>("PuppyPreviewImage");
        }

        if (puppyPreviewImage == null && createMissingPreviewFallbacks)
        {
            puppyPreviewImage = CreateMissingPuppyPreviewImageFallback();
        }
    }

    void FindParentPreviewTextReferences()
    {
        if (parent1PreviewText == null)
        {
            parent1PreviewText = FindSceneComponentByName<TextMeshProUGUI>("Parent1PreviewText");
        }

        if (parent2PreviewText == null)
        {
            parent2PreviewText = FindSceneComponentByName<TextMeshProUGUI>("Parent2PreviewText");
        }

        if (parent1PreviewText == null && createMissingPreviewFallbacks)
        {
            parent1PreviewText = CreateMissingParentPreviewTextFallback("Parent1PreviewText", parent1PortraitImage, true);
        }

        if (parent2PreviewText == null && createMissingPreviewFallbacks)
        {
            parent2PreviewText = CreateMissingParentPreviewTextFallback("Parent2PreviewText", parent2PortraitImage, false);
        }
    }

    Image CreateMissingPuppyPreviewImageFallback()
    {
        RectTransform centerPanel = FindSceneComponentByName<RectTransform>("BreedCenterPanel1");
        Transform previewParent = centerPanel != null ? centerPanel.transform : transform;

        GameObject imageObject = new GameObject("PuppyPreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(previewParent, false);
        imageObject.transform.SetAsFirstSibling();

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 14f);
        rectTransform.sizeDelta = new Vector2(70f, 70f);
        rectTransform.localScale = Vector3.one;

        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.enabled = false;

        return image;
    }

    TextMeshProUGUI CreateMissingParentPreviewTextFallback(string objectName, Image anchorImage, bool isLeftSide)
    {
        Transform textParent = anchorImage != null && anchorImage.transform.parent != null
            ? anchorImage.transform.parent
            : transform;

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(textParent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        RectTransform rectTransform = text.rectTransform;
        Vector2 textSize = new Vector2(230f, 150f);

        if (anchorImage != null)
        {
            RectTransform anchorRect = anchorImage.rectTransform;
            float portraitWidth = Mathf.Max(70f, Mathf.Abs(anchorRect.sizeDelta.x));
            float horizontalGap = (portraitWidth * 0.5f) + (textSize.x * 0.5f) + 18f;

            rectTransform.anchorMin = anchorRect.anchorMin;
            rectTransform.anchorMax = anchorRect.anchorMax;
            rectTransform.pivot = anchorRect.pivot;
            rectTransform.anchoredPosition = anchorRect.anchoredPosition + new Vector2(isLeftSide ? horizontalGap : -horizontalGap, -8f);
        }
        else
        {
            rectTransform.anchorMin = isLeftSide ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.pivot = isLeftSide ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rectTransform.anchoredPosition = isLeftSide ? new Vector2(24f, 0f) : new Vector2(-24f, 0f);
        }

        rectTransform.sizeDelta = textSize;

        return text;
    }

    void ConfigurePuppyPreviewText()
    {
        if (puppyPreviewText == null)
        {
            return;
        }

        puppyPreviewText.raycastTarget = false;
        puppyPreviewText.alignment = TextAlignmentOptions.TopLeft;
        puppyPreviewText.textWrappingMode = TextWrappingModes.Normal;
        puppyPreviewText.overflowMode = TextOverflowModes.Truncate;
    }

    T FindSceneComponentByName<T>(string objectName) where T : Component
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        T[] components = Resources.FindObjectsOfTypeAll<T>();

        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];

            if (component == null ||
                component.gameObject == null ||
                !component.gameObject.scene.IsValid())
            {
                continue;
            }

            if (component.gameObject.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    void ConfigureParentPreviewText(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.fontSize = 13f;
        text.color = new Color(0.9f, 0.96f, 1f, 1f);
    }

    void SetParentPreviewText(TextMeshProUGUI text, Dog selectedDog)
    {
        if (text == null)
        {
            return;
        }

        text.text = BuildParentPreviewText(selectedDog);
    }

    void UpdatePuppyPreview(Dog parent1, Dog parent2)
    {
        string previewText = BuildPuppyPreviewText(parent1, parent2, out string predictedBreed);
        SetPuppyPreviewText(previewText);

        Sprite previewSprite = ChoosePuppyPreviewSprite(parent1, parent2, predictedBreed);
        SetPreviewImage(puppyPreviewImage, previewSprite);
    }

    string BuildPuppyPreviewText(Dog parent1, Dog parent2, out string predictedBreed)
    {
        predictedBreed = string.Empty;

        if (parent1 == null || parent2 == null)
        {
            return
                "<color=#7CF6FF><b>PREDICTED PUPPY PREVIEW</b></color>\n" +
                "No valid pair selected.\n" +
                "Select one sire and one dam to preview a possible puppy.\n" +
                "Preview only - no dog is created or saved.";
        }

        predictedBreed = GetDisplayBreedName(BreedLibrary.GetHybridBreedName(parent1.breed, parent2.breed));

        string warning = GetPuppyPreviewWarning(parent1, parent2);

        if (!string.IsNullOrEmpty(warning))
        {
            return
                "<color=#7CF6FF><b>PREDICTED PUPPY PREVIEW</b></color>\n" +
                "Preview only - no dog is created or saved.\n" +
                $"Predicted Breed: {predictedBreed}\n" +
                BuildGenerationPreviewLine(parent1, parent2) +
                BuildParentMixLine(parent1, parent2) +
                warning;
        }

        return
            "<color=#7CF6FF><b>PREDICTED PUPPY PREVIEW</b></color>\n" +
            "Preview only - no dog is created or saved.\n" +
            $"Predicted Breed: {predictedBreed}\n" +
            $"Name Style: {BuildPredictedNameStyleLine(parent1, parent2)}\n" +
            BuildGenerationPreviewLine(parent1, parent2) +
            BuildParentMixLine(parent1, parent2) +
            BuildLikelyStrategyPreviewLine(parent1, parent2) +
            BuildLikelyTraitPreviewLine(parent1, parent2) +
            BuildLikelyStatRangePreviewLine(parent1, parent2) +
            $"Preview Image: {GetBreedFamilyLabel(predictedBreed, parent1, parent2)}";
    }

    string BuildParentPreviewText(Dog dog)
    {
        if (dog == null)
        {
            return "<color=#7CF6FF><b>EMPTY SLOT</b></color>\nSelect a breeding dog.";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"<color=#7CF6FF><b>{GetParentRoleLabel(dog)}</b></color>");
        builder.AppendLine(GetSafeDogName(dog));
        builder.AppendLine($"{dog.gender} - {GetDisplayBreedName(dog.breed)}");
        builder.AppendLine($"STR {dog.strength}/{dog.strengthPotential}  AGI {dog.agility}/{dog.agilityPotential}");
        builder.AppendLine($"STA {dog.stamina}/{dog.staminaPotential}  INT {dog.GetIntelligence()}/{dog.GetIntelligencePotential()}");
        builder.AppendLine($"Strategy: {GetBreedingPreviewStrategyText(dog)}");
        builder.Append($"Traits: {BuildKeyTraitText(dog)}");
        return builder.ToString();
    }

    string GetParentRoleLabel(Dog dog)
    {
        if (dog == null)
        {
            return "Parent";
        }

        return dog.gender == DogGender.Male ? "Sire" : "Dam";
    }

    string GetSafeDogName(Dog dog)
    {
        return dog == null || string.IsNullOrWhiteSpace(dog.dogName) ? "Unnamed Dog" : dog.dogName.Trim();
    }

    string BuildKeyTraitText(Dog dog)
    {
        if (dog == null)
        {
            return "Unknown";
        }

        string traitSummary = dog.GetTraitSummary();
        return traitSummary == "No Traits" ? "None discovered" : traitSummary;
    }

    string GetBreedingPreviewStrategyText(Dog dog)
    {
        if (dogManager != null && dog != null)
        {
            return dogManager.GetCurrentStrategyTextForDog(dog);
        }

        return "Unknown";
    }

    string BuildPredictedNameStyleLine(Dog parent1, Dog parent2)
    {
        return $"{GetSafeDogName(parent1)} / {GetSafeDogName(parent2)} bloodline";
    }

    string BuildLikelyStrategyPreviewLine(Dog parent1, Dog parent2)
    {
        string strategyA = GetBreedingPreviewStrategyText(parent1);
        string strategyB = GetBreedingPreviewStrategyText(parent2);

        if (strategyA == "Unknown" && strategyB == "Unknown")
        {
            return string.Empty;
        }

        if (strategyA == strategyB)
        {
            return $"Likely Strategy: {strategyA}\n";
        }

        return $"Likely Strategy: {strategyA} / {strategyB}\n";
    }

    string BuildGenerationPreviewLine(Dog parent1, Dog parent2)
    {
        int predictedGeneration = Mathf.Max(parent1.generation, parent2.generation) + 1;
        return $"Generation: G{predictedGeneration + 1}\n";
    }

    string BuildParentMixLine(Dog parent1, Dog parent2)
    {
        return
            $"Parent Mix: {GetParentMixLabel(parent1)} x {GetParentMixLabel(parent2)}\n";
    }

    string GetParentMixLabel(Dog parent)
    {
        if (parent == null)
        {
            return "Unknown";
        }

        string parentName = string.IsNullOrWhiteSpace(parent.dogName)
            ? "Unknown"
            : parent.dogName.Trim();
        string parentBreed = string.IsNullOrWhiteSpace(parent.breed)
            ? "Unknown"
            : GetDisplayBreedName(parent.breed);

        return $"{parentName} ({parentBreed})";
    }

    string BuildLikelyTraitPreviewLine(Dog parent1, Dog parent2)
    {
        string inheritedTraits = BuildLikelyInheritedTraitText(parent1, parent2);
        int mutationPercent = Mathf.RoundToInt(traitMutationChance * 100f);

        return $"Likely Traits: {inheritedTraits}; {mutationPercent}% mutation chance\n";
    }

    string BuildLikelyInheritedTraitText(Dog parent1, Dog parent2)
    {
        System.Collections.Generic.List<string> traitNames = new System.Collections.Generic.List<string>();
        AddTraitNameIfMissing(traitNames, parent1.primaryTrait);
        AddTraitNameIfMissing(traitNames, parent1.secondaryTrait);
        AddTraitNameIfMissing(traitNames, parent2.primaryTrait);
        AddTraitNameIfMissing(traitNames, parent2.secondaryTrait);

        if (traitNames.Count == 0)
        {
            return "None from parents";
        }

        return string.Join("/", traitNames.ToArray());
    }

    void AddTraitNameIfMissing(System.Collections.Generic.List<string> traitNames, DogTrait trait)
    {
        if (traitNames == null || trait == DogTrait.None)
        {
            return;
        }

        string traitName = trait.ToString();

        if (!traitNames.Contains(traitName))
        {
            traitNames.Add(traitName);
        }
    }

    string BuildLikelyStatRangePreviewLine(Dog parent1, Dog parent2)
    {
        return
            "Likely Stat Range:\n" +
            $"STR {BuildStartingStatRange(parent1.strength, parent2.strength, parent1.strengthPotential, parent2.strengthPotential)}  " +
            $"AGI {BuildStartingStatRange(parent1.agility, parent2.agility, parent1.agilityPotential, parent2.agilityPotential)}\n" +
            $"STA {BuildStartingStatRange(parent1.stamina, parent2.stamina, parent1.staminaPotential, parent2.staminaPotential)}  " +
            $"INT {BuildStartingStatRange(parent1.GetIntelligence(), parent2.GetIntelligence(), parent1.GetIntelligencePotential(), parent2.GetIntelligencePotential())}\n";
    }

    string GetPuppyPreviewWarning(Dog parent1, Dog parent2)
    {
        if (parent1 == parent2)
        {
            return "Warning: choose two different dogs.";
        }

        if (parent1.isDead || parent2.isDead)
        {
            return "Warning: deceased dogs cannot breed.";
        }

        if (parent1.isRetired || parent2.isRetired)
        {
            return "Warning: retired dogs cannot breed.";
        }

        if (parent1.gender == parent2.gender)
        {
            return "Warning: needs one male and one female.";
        }

        if (dogManager != null &&
            (!parent1.CanBreedInWeek(dogManager.currentWeek) ||
             !parent2.CanBreedInWeek(dogManager.currentWeek)))
        {
            return "Warning: breeding cooldown active.";
        }

        return string.Empty;
    }

    string BuildPotentialPreviewLine(Dog parent1, Dog parent2)
    {
        return
            $"Pot: STR {BuildPotentialRange(parent1.strengthPotential, parent2.strengthPotential)}  " +
            $"AGI {BuildPotentialRange(parent1.agilityPotential, parent2.agilityPotential)}\n" +
            $"STA {BuildPotentialRange(parent1.staminaPotential, parent2.staminaPotential)}  " +
            $"INT {BuildPotentialRange(parent1.GetIntelligencePotential(), parent2.GetIntelligencePotential())}";
    }

    string BuildStartingStatRange(int parentStat1, int parentStat2, int parentPotential1, int parentPotential2)
    {
        int average = Mathf.RoundToInt((parentStat1 + parentStat2) / 2f);
        int potentialHigh = GetPotentialPreviewHigh(parentPotential1, parentPotential2);
        int upperLimit = Mathf.Min(100, potentialHigh);
        int low = Mathf.Clamp(average - statVariance, 1, upperLimit);
        int high = Mathf.Clamp(average + statVariance, 1, upperLimit);

        if (high < low)
        {
            high = low;
        }

        return $"{low}-{high}";
    }

    string BuildPotentialRange(int parentPotential1, int parentPotential2)
    {
        int inheritedPotential = GetInheritedPotentialPreviewCenter(parentPotential1, parentPotential2);
        int low = Mathf.Clamp(inheritedPotential - potentialVariance, 40, 120);
        int high = GetPotentialPreviewHigh(parentPotential1, parentPotential2);

        return $"{low}-{high}";
    }

    int GetPotentialPreviewHigh(int parentPotential1, int parentPotential2)
    {
        int inheritedPotential = GetInheritedPotentialPreviewCenter(parentPotential1, parentPotential2);
        return Mathf.Clamp(inheritedPotential + potentialVariance, 40, 120);
    }

    int GetInheritedPotentialPreviewCenter(int parentPotential1, int parentPotential2)
    {
        int bestParentPotential = Mathf.Max(parentPotential1, parentPotential2);
        int average = Mathf.RoundToInt((parentPotential1 + parentPotential2) / 2f);
        int inheritanceFloor = Mathf.RoundToInt(bestParentPotential * 0.85f);
        return Mathf.Max(average, inheritanceFloor);
    }

    Sprite ChoosePuppyPreviewSprite(Dog parent1, Dog parent2, string predictedBreed)
    {
        if (string.IsNullOrWhiteSpace(predictedBreed))
        {
            return defaultDogPortraitSprite;
        }

        string previewIdentity = $"{GetDogIdentityKey(parent1)}|{GetDogIdentityKey(parent2)}|preview";
        Sprite archetypeSprite = DogPortraitLibrary.ChooseBreedArchetypePortrait(previewIdentity, predictedBreed);

        if (archetypeSprite != null)
        {
            return archetypeSprite;
        }

        Sprite parentFallback1 = parent1 != null ? parent1.dogSprite : null;
        Sprite parentFallback2 = parent2 != null ? parent2.dogSprite : null;
        Sprite puppySprite = DogPortraitLibrary.ChoosePuppyPortrait(
            previewIdentity,
            predictedBreed,
            parentFallback1,
            parentFallback2);

        return puppySprite != null ? puppySprite : defaultDogPortraitSprite;
    }

    string GetDogIdentityKey(Dog dog)
    {
        if (dog == null)
        {
            return "dog";
        }

        if (!string.IsNullOrWhiteSpace(dog.dogId))
        {
            return dog.dogId;
        }

        if (!string.IsNullOrWhiteSpace(dog.dogName))
        {
            return dog.dogName;
        }

        return "dog";
    }

    void SetPuppyPreviewText(string message)
    {
        if (puppyPreviewText != null)
        {
            puppyPreviewText.text = message;
        }
    }

    void SetPreviewImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.color = Color.white;
        image.enabled = sprite != null;
        image.raycastTarget = false;
        image.preserveAspect = true;
    }

    string GetBreedFamilyLabel(string breedName, Dog parent1, Dog parent2)
    {
        bool parentHybrid = IsHybridBreed(parent1, parent2);

        if ((parentHybrid || IsHybridBreedText(breedName)) &&
            ContainsShepherdBreedText(breedName) &&
            ContainsBullyBreedText(breedName))
        {
            return "Shepherd/Bully Hybrid";
        }

        if (parentHybrid || IsHybridBreedText(breedName))
        {
            return "Hybrid Variant";
        }

        if (ContainsBullyBreedText(breedName))
        {
            return "Bully Striker";
        }

        if (ContainsShepherdBreedText(breedName))
        {
            return "Shepherd Sentinel";
        }

        if (ContainsGuardMastiffBreedText(breedName))
        {
            return "Guard Mastiff";
        }

        if (ContainsIronRottBreedText(breedName))
        {
            return "Iron Rott";
        }

        if (ContainsSpitzBreedText(breedName))
        {
            return "Spitz Warden";
        }

        if (ContainsHoundBreedText(breedName))
        {
            return "Velocity Hound";
        }

        return "Unknown Archetype";
    }

    bool IsHybridBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string compactBreed = GetCompactBreedName(breedName);

        return rawBreed.Contains("hybrid") ||
               rawBreed.Contains("mix") ||
               rawBreed.Contains("cross") ||
               rawBreed.Contains(" x ") ||
               compactBreed.Contains("hybrid") ||
               compactBreed.Contains("mixed") ||
               compactBreed.Contains("cross");
    }

    bool ContainsBullyBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string compactBreed = GetCompactBreedName(breedName);

        return rawBreed.Contains("pit bull") ||
               compactBreed.Contains("pitbull") ||
               rawBreed.Contains("bully") ||
               compactBreed.Contains("bully") ||
               rawBreed.Contains("boxer") ||
               compactBreed.Contains("boxer") ||
               rawBreed.Contains("bulldog") ||
               compactBreed.Contains("bulldog") ||
               compactBreed.Contains("bull");
    }

    bool ContainsShepherdBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string compactBreed = GetCompactBreedName(breedName);

        return rawBreed.Contains("german shepherd") ||
               rawBreed.Contains("german shepard") ||
               rawBreed.Contains("shepherd") ||
               rawBreed.Contains("shepard") ||
               rawBreed.Contains("malinois") ||
               rawBreed.Contains("tervuren") ||
               compactBreed.Contains("german");
    }

    bool ContainsGuardMastiffBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string compactBreed = GetCompactBreedName(breedName);

        return rawBreed.Contains("mastiff") ||
               compactBreed.Contains("mastiff") ||
               rawBreed.Contains("cane corso") ||
               compactBreed.Contains("canecorso") ||
               rawBreed.Contains("presa") ||
               rawBreed.Contains("dogo") ||
               rawBreed.Contains("boerboel") ||
               rawBreed.Contains("kangal") ||
               rawBreed.Contains("tosa") ||
               rawBreed.Contains("fila") ||
               rawBreed.Contains("great dane") ||
               compactBreed.Contains("centralasianshepherd");
    }

    bool ContainsIronRottBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string compactBreed = GetCompactBreedName(breedName);

        return rawBreed.Contains("rottweiler") ||
               rawBreed.Contains("rott") ||
               rawBreed.Contains("doberman") ||
               rawBreed.Contains("beauceron") ||
               compactBreed.Contains("blackrussianterrier");
    }

    bool ContainsSpitzBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);

        return rawBreed.Contains("akita") ||
               rawBreed.Contains("spitz") ||
               rawBreed.Contains("husky") ||
               rawBreed.Contains("malamute") ||
               rawBreed.Contains("shiba") ||
               rawBreed.Contains("chow") ||
               rawBreed.Contains("samoyed");
    }

    bool ContainsHoundBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);

        return rawBreed.Contains("greyhound") ||
               rawBreed.Contains("whippet") ||
               rawBreed.Contains("saluki") ||
               rawBreed.Contains("ridgeback") ||
               rawBreed.Contains("pharaoh") ||
               rawBreed.Contains("catahoula") ||
               rawBreed.Contains("hound");
    }

    string GetRawBreedText(string breedName)
    {
        return string.IsNullOrWhiteSpace(breedName)
            ? string.Empty
            : breedName.Trim().ToLowerInvariant();
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
