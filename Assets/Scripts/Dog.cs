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
    AllIn
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

[CreateAssetMenu(fileName = "NewDog", menuName = "Dogs/Dog")]
public class Dog : ScriptableObject
{
    [Header("Identity")]
    public string dogId = "dog_id";
    public string dogName = "New Dog";
    public string breed = "Pit Bull";

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

    [Header("Genetics")]
    public int generation = 0;
    public string parent1Id = "";
    public string parent2Id = "";

    [Header("Base Stats")]
    [Range(1, 100)] public int strength = 70;
    [Range(1, 100)] public int agility = 65;
    [Range(1, 100)] public int stamina = 75;

    [Header("Potential Ceilings")]
    [Range(1, 120)] public int strengthPotential = 100;
    [Range(1, 120)] public int agilityPotential = 100;
    [Range(1, 120)] public int staminaPotential = 100;

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
        return Mathf.RoundToInt((strengthPotential + agilityPotential + staminaPotential) / 3f);
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

        int highestPotential = Mathf.Max(strengthPotential, agilityPotential, staminaPotential);

        bool strengthDominant = strengthPotential == highestPotential;
        bool agilityDominant = agilityPotential == highestPotential;
        bool staminaDominant = staminaPotential == highestPotential;

        if (strengthDominant && !agilityDominant && !staminaDominant)
        {
            return $"{tier} Crusher";
        }

        if (agilityDominant && !strengthDominant && !staminaDominant)
        {
            return $"{tier} Phantom";
        }

        if (staminaDominant && !strengthDominant && !agilityDominant)
        {
            return $"{tier} Ironhide";
        }

        if (strengthDominant && agilityDominant && staminaDominant)
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
}
