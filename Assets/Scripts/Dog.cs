using UnityEngine;

public enum FightStyle
{
    Balanced,
    Rushdown,
    Counter,
    Tank,
    Wildcard
}

public enum FightStrategy
{
    Balanced,
    RushEarly,
    CounterPlan,
    WearDown,
    DefensiveShell,
    AllIn,
    SecondWind
}

public enum DogTrait
{
    None,
    Aggressive,
    Durable,
    GlassCannon,
    Clutch,
    LateBloomer,
    Prodigy
}

public enum DogGender
{
    Male,
    Female
}

[CreateAssetMenu(fileName = "NewDog", menuName = "Dogs/Dog")]
public class Dog : ScriptableObject
{
    public const int DefaultIntelligence = 12;
    public const int DefaultIntelligencePotential = 100;

    [Header("Identity")]
    public string dogId = "dog_id";
    public string dogName = "New Dog";
    public string breed = "Pit Bull";
    public DogGender gender = DogGender.Male;

    [Header("Age / Career")]
    public int age = 0;
    public bool isDead = false;
    public bool isRetired = false;

    public string GetLifeStage()
    {
        if (age <= 2) return "Young";
        if (age <= 8) return "Developing";
        if (age <= 18) return "Prime";
        if (age <= 25) return "Veteran";
        return "Elder";
    }

    public bool CanFight()
    {
        return !isDead && !isRetired;
    }

    public bool CanBreed()
    {
        return !isDead && !isRetired;
    }

    public bool CanBreedInWeek(int currentWeek)
    {
        return CanBreed() && currentWeek > lastBredWeek;
    }

    public bool CanBreedWith(Dog otherDog, int currentWeek)
    {
        if (otherDog == null)
        {
            return false;
        }

        return CanBreedInWeek(currentWeek) &&
               otherDog.CanBreedInWeek(currentWeek) &&
               gender != otherDog.gender;
    }

    public string GetStatusText()
    {
        if (isDead)
        {
            return "Deceased";
        }

        if (isRetired)
        {
            return "Retired";
        }

        return "Active";
    }

    [Header("Genetics")]
    public int generation = 0;
    public string parentAId = "";
    public string parentBId = "";
    public string parent1Id = "";
    public string parent2Id = "";
    public string fatherId = "";
    public string motherId = "";
    public string parentAName = "";
    public string parentBName = "";
    public string parentABreed = "";
    public string parentBBreed = "";
    public string parentASex = "";
    public string parentBSex = "";
    public int lastBredWeek = -999;

    [Header("Bloodline")]
    public string bloodlineName = "";
    public int ancestorStrengthBonus = 0;
    public int ancestorAgilityBonus = 0;
    public int ancestorStaminaBonus = 0;
    public string ancestorBonusSummary = "";
    public bool isBloodlineCarrier = false;

    [Header("Base Stats")]
    [Range(1, 100)] public int strength = 70;
    [Range(1, 100)] public int agility = 65;
    [Range(1, 100)] public int stamina = 75;
    [Range(1, 100)] public int intelligence = DefaultIntelligence;

    [Header("Potential Ceilings")]
    [Range(1, 120)] public int strengthPotential = 100;
    [Range(1, 120)] public int agilityPotential = 100;
    [Range(1, 120)] public int staminaPotential = 100;
    [Range(1, 120)] public int intelligencePotential = DefaultIntelligencePotential;

    [Header("Growth")]
    [Range(0.5f, 2.0f)] public float growthRate = 1.0f;

    [Header("Style")]
    public FightStyle fightStyle = FightStyle.Balanced;

    [Header("Traits")]
    public DogTrait primaryTrait = DogTrait.None;
    public DogTrait secondaryTrait = DogTrait.None;

    [Header("Progression")]
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 100;

    [Header("Record")]
    public int wins = 0;
    public int losses = 0;
    public int totalFights = 0;

    [Header("Visuals")]
    public Sprite dogSprite;

    public int GetPotentialScore()
    {
        return Mathf.RoundToInt((strengthPotential + agilityPotential + staminaPotential + GetIntelligencePotential()) / 4f);
    }

    public string GetPotentialTier()
    {
        int score = GetPotentialScore();

        if (score >= 115)
        {
            return "Legendary";
        }

        if (score >= 105)
        {
            return "Apex";
        }

        if (score >= 95)
        {
            return "Elite";
        }

        if (score >= 85)
        {
            return "Contender";
        }

        if (score >= 70)
        {
            return "Prospect";
        }

        return "Street";
    }

    public string GetPotentialTitle()
    {
        string tier = GetPotentialTier();

        int safeIntelligencePotential = GetIntelligencePotential();
        int highestPotential = Mathf.Max(strengthPotential, agilityPotential, staminaPotential, safeIntelligencePotential);

        bool strengthDominant = strengthPotential == highestPotential;
        bool agilityDominant = agilityPotential == highestPotential;
        bool staminaDominant = staminaPotential == highestPotential;
        bool intelligenceDominant = safeIntelligencePotential == highestPotential;

        if (strengthDominant && !agilityDominant && !staminaDominant && !intelligenceDominant)
        {
            return $"{tier} Crusher";
        }

        if (agilityDominant && !strengthDominant && !staminaDominant && !intelligenceDominant)
        {
            return $"{tier} Phantom";
        }

        if (staminaDominant && !strengthDominant && !agilityDominant && !intelligenceDominant)
        {
            return $"{tier} Ironhide";
        }

        if (intelligenceDominant && !strengthDominant && !agilityDominant && !staminaDominant)
        {
            return $"{tier} Tactician";
        }

        if (strengthDominant && agilityDominant && staminaDominant && intelligenceDominant)
        {
            return $"{tier} Perfect Prospect";
        }

        return $"{tier} Hybrid";
    }

    public string GetTraitSummary()
    {
        bool hasPrimaryTrait = primaryTrait != DogTrait.None;
        bool hasSecondaryTrait = secondaryTrait != DogTrait.None;

        if (!hasPrimaryTrait && !hasSecondaryTrait)
        {
            return "No Traits";
        }

        if (hasPrimaryTrait && (!hasSecondaryTrait || secondaryTrait == primaryTrait))
        {
            return primaryTrait.ToString();
        }

        if (!hasPrimaryTrait)
        {
            return secondaryTrait.ToString();
        }

        return $"{primaryTrait} / {secondaryTrait}";
    }

    public bool HasTrait(DogTrait trait)
    {
        if (trait == DogTrait.None)
        {
            return false;
        }

        return primaryTrait == trait || secondaryTrait == trait;
    }

    public int GetIntelligence()
    {
        return Mathf.Clamp(intelligence <= 0 ? DefaultIntelligence : intelligence, 1, 100);
    }

    public int GetIntelligencePotential()
    {
        return Mathf.Clamp(intelligencePotential <= 0 ? DefaultIntelligencePotential : intelligencePotential, 1, 120);
    }

    public void NormalizeLegacyStats()
    {
        if (intelligence <= 0)
        {
            intelligence = DefaultIntelligence;
        }

        if (intelligencePotential <= 0)
        {
            intelligencePotential = DefaultIntelligencePotential;
        }
    }
}
