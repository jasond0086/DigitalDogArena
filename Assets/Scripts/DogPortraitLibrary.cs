using System;
using System.Collections.Generic;
using UnityEngine;

public static class DogPortraitLibrary
{
    private const string PortraitResourceRoot = "DogPortraits";
    private const string BreedArchetypeResourceRoot = "FightPresentation/BreedArchetypes";

    private enum BreedPortraitArchetype
    {
        ShepherdSentinel,
        BullyStriker,
        GuardMastiff,
        IronRott,
        SpitzWarden,
        VelocityHound,
        HybridVariant,
        Unknown
    }

    private static readonly Dictionary<string, Sprite> cachedBreedArchetypeSprites =
        new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> missingBreedArchetypeResources =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static string GetBreedResourceKey(string breedName)
    {
        string normalizedBreed = NormalizeBreedName(breedName);
        return normalizedBreed.Replace(' ', '_');
    }

    public static Sprite[] LoadBreedPortraits(string breedName)
    {
        string breedKey = GetBreedResourceKey(breedName);
        return Resources.LoadAll<Sprite>($"{PortraitResourceRoot}/{breedKey}");
    }

    public static Sprite ChoosePuppyPortrait(
        string puppyIdOrName,
        string breed,
        Sprite fatherSprite,
        Sprite motherSprite)
    {
        Sprite[] breedPortraits = LoadBreedPortraits(breed);

        if (breedPortraits != null && breedPortraits.Length > 0)
        {
            int portraitIndex = GetDeterministicIndex($"{puppyIdOrName}|{breed}", breedPortraits.Length);
            return breedPortraits[portraitIndex];
        }

        if (fatherSprite != null && motherSprite != null)
        {
            int parentIndex = GetDeterministicIndex($"{puppyIdOrName}|{breed}|parents", 2);
            return parentIndex == 0 ? fatherSprite : motherSprite;
        }

        if (fatherSprite != null)
        {
            return fatherSprite;
        }

        if (motherSprite != null)
        {
            return motherSprite;
        }

        return null;
    }

    public static Sprite ChooseStableCardPortrait(Dog dog, Sprite oldPortraitFallback, Sprite defaultFallback)
    {
        if (dog != null)
        {
            Sprite breedArchetypePortrait = ChooseBreedArchetypePortrait(GetDogIdentityKey(dog), dog.breed);

            if (breedArchetypePortrait != null)
            {
                return breedArchetypePortrait;
            }
        }

        if (oldPortraitFallback != null)
        {
            return oldPortraitFallback;
        }

        return defaultFallback;
    }

    public static Sprite ChooseBreedArchetypePortrait(string dogIdOrName, string breed)
    {
        List<string> resourceNames = GetBreedArchetypeResourceNames(dogIdOrName, breed);

        for (int i = 0; i < resourceNames.Count; i++)
        {
            Sprite sprite = LoadBreedArchetypeSprite(resourceNames[i]);

            if (sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    static List<string> GetBreedArchetypeResourceNames(string dogIdOrName, string breed)
    {
        List<string> resourceNames = new List<string>();

        if (IsShepherdBullyHybridText(breed))
        {
            AddResourceName(resourceNames, "dog_imprint_shepherd_hybrid_variant_01");
            AddResourceName(resourceNames, "dog_imprint_bully_hybrid_variant_01");
            AddResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        bool isHybrid = IsHybridBreedText(breed);

        if (isHybrid)
        {
            if (ContainsBullyBreedText(breed))
            {
                AddResourceName(resourceNames, "dog_imprint_bully_hybrid_variant_01");
                AddVariantResourceNames(resourceNames, BreedPortraitArchetype.BullyStriker, dogIdOrName, breed);
                AddResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
                return resourceNames;
            }

            if (ContainsShepherdBreedText(breed))
            {
                AddResourceName(resourceNames, "dog_imprint_shepherd_hybrid_variant_01");
                AddVariantResourceNames(resourceNames, BreedPortraitArchetype.ShepherdSentinel, dogIdOrName, breed);
                AddResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
                return resourceNames;
            }

            AddResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        BreedPortraitArchetype archetype = ResolveBreedPortraitArchetype(breed);

        if (archetype == BreedPortraitArchetype.HybridVariant)
        {
            AddResourceName(resourceNames, "dog_imprint_hybrid_variant_01");
            return resourceNames;
        }

        AddVariantResourceNames(resourceNames, archetype, dogIdOrName, breed);
        return resourceNames;
    }

    static void AddVariantResourceNames(
        List<string> resourceNames,
        BreedPortraitArchetype archetype,
        string dogIdOrName,
        string breed)
    {
        string baseName = GetBreedArchetypeResourceBaseName(archetype);

        if (string.IsNullOrEmpty(baseName))
        {
            return;
        }

        int firstVariant = GetDeterministicIndex($"{dogIdOrName}|{breed}|{baseName}", 2) + 1;
        int secondVariant = firstVariant == 1 ? 2 : 1;

        AddResourceName(resourceNames, $"{baseName}_{firstVariant:00}");
        AddResourceName(resourceNames, $"{baseName}_{secondVariant:00}");
    }

    static void AddResourceName(List<string> resourceNames, string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName) || resourceNames.Contains(resourceName))
        {
            return;
        }

        resourceNames.Add(resourceName);
    }

    static Sprite LoadBreedArchetypeSprite(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            return null;
        }

        if (cachedBreedArchetypeSprites.TryGetValue(resourceName, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        if (missingBreedArchetypeResources.Contains(resourceName))
        {
            return null;
        }

        string resourcePath = $"{BreedArchetypeResourceRoot}/{resourceName}";
        Sprite sprite = Resources.Load<Sprite>(resourcePath);

        if (sprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);

            if (texture != null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        if (sprite != null)
        {
            cachedBreedArchetypeSprites[resourceName] = sprite;
            return sprite;
        }

        missingBreedArchetypeResources.Add(resourceName);
        return null;
    }

    static BreedPortraitArchetype ResolveBreedPortraitArchetype(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return BreedPortraitArchetype.Unknown;
        }

        string rawBreed = GetRawNormalizedBreedText(breed);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breed);
        string compactBreed = GetCompactBreedText(breed);

        if (rawBreed.Contains("wolfdog") || compactBreed.Contains("wolfdog"))
        {
            return BreedPortraitArchetype.HybridVariant;
        }

        if (separatorNormalizedBreed.Contains("central asian shepherd") ||
            compactBreed.Contains("centralasianshepherd"))
        {
            return BreedPortraitArchetype.GuardMastiff;
        }

        if (ContainsShepherdBreedText(breed))
        {
            return BreedPortraitArchetype.ShepherdSentinel;
        }

        if (ContainsBullyBreedText(breed))
        {
            return BreedPortraitArchetype.BullyStriker;
        }

        if (rawBreed.Contains("mastiff") ||
            compactBreed.Contains("mastiff") ||
            separatorNormalizedBreed.Contains("cane corso") ||
            compactBreed.Contains("canecorso") ||
            rawBreed.Contains("presa") ||
            compactBreed.Contains("presa") ||
            separatorNormalizedBreed.Contains("dogo argentino") ||
            compactBreed.Contains("dogoargentino") ||
            rawBreed.Contains("boerboel") ||
            compactBreed.Contains("boerboel") ||
            separatorNormalizedBreed.Contains("great dane") ||
            compactBreed.Contains("greatdane") ||
            rawBreed.Contains("kangal") ||
            compactBreed.Contains("kangal") ||
            rawBreed.Contains("tosa") ||
            compactBreed.Contains("tosa") ||
            rawBreed.Contains("fila") ||
            compactBreed.Contains("fila"))
        {
            return BreedPortraitArchetype.GuardMastiff;
        }

        if (rawBreed.Contains("rottweiler") ||
            compactBreed.Contains("rottweiler") ||
            rawBreed.Contains("rott") ||
            compactBreed.Contains("rott") ||
            rawBreed.Contains("doberman") ||
            compactBreed.Contains("doberman") ||
            rawBreed.Contains("beauceron") ||
            compactBreed.Contains("beauceron") ||
            separatorNormalizedBreed.Contains("black russian terrier") ||
            compactBreed.Contains("blackrussianterrier"))
        {
            return BreedPortraitArchetype.IronRott;
        }

        if (rawBreed.Contains("akita") ||
            compactBreed.Contains("akita") ||
            rawBreed.Contains("spitz") ||
            compactBreed.Contains("spitz") ||
            rawBreed.Contains("husky") ||
            compactBreed.Contains("husky") ||
            rawBreed.Contains("malamute") ||
            compactBreed.Contains("malamute") ||
            rawBreed.Contains("shiba") ||
            compactBreed.Contains("shiba") ||
            rawBreed.Contains("chow") ||
            compactBreed.Contains("chow") ||
            rawBreed.Contains("samoyed") ||
            compactBreed.Contains("samoyed"))
        {
            return BreedPortraitArchetype.SpitzWarden;
        }

        if (rawBreed.Contains("greyhound") ||
            compactBreed.Contains("greyhound") ||
            rawBreed.Contains("whippet") ||
            compactBreed.Contains("whippet") ||
            rawBreed.Contains("saluki") ||
            compactBreed.Contains("saluki") ||
            rawBreed.Contains("ridgeback") ||
            compactBreed.Contains("ridgeback") ||
            rawBreed.Contains("pharaoh") ||
            compactBreed.Contains("pharaoh") ||
            rawBreed.Contains("catahoula") ||
            compactBreed.Contains("catahoula") ||
            rawBreed.Contains("hound") ||
            compactBreed.Contains("hound"))
        {
            return BreedPortraitArchetype.VelocityHound;
        }

        return IsHybridBreedText(breed)
            ? BreedPortraitArchetype.HybridVariant
            : BreedPortraitArchetype.Unknown;
    }

    static string GetBreedArchetypeResourceBaseName(BreedPortraitArchetype archetype)
    {
        switch (archetype)
        {
            case BreedPortraitArchetype.ShepherdSentinel:
                return "dog_imprint_shepherd_sentinel";

            case BreedPortraitArchetype.BullyStriker:
                return "dog_imprint_bully_striker";

            case BreedPortraitArchetype.GuardMastiff:
                return "dog_imprint_guard_mastiff";

            case BreedPortraitArchetype.IronRott:
                return "dog_imprint_iron_rott";

            case BreedPortraitArchetype.SpitzWarden:
                return "dog_imprint_spitz_warden";

            case BreedPortraitArchetype.VelocityHound:
                return "dog_imprint_velocity_hound";

            case BreedPortraitArchetype.HybridVariant:
                return "dog_imprint_hybrid_variant";

            case BreedPortraitArchetype.Unknown:
            default:
                return string.Empty;
        }
    }

    static bool ContainsBullyBreedText(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breed);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breed);
        string compactBreed = GetCompactBreedText(breed);

        return separatorNormalizedBreed.Contains("pit bull") ||
               compactBreed.Contains("pitbull") ||
               rawBreed.Contains("american staffordshire") ||
               rawBreed.Contains("staffordshire") ||
               separatorNormalizedBreed.Contains("bull terrier") ||
               compactBreed.Contains("bullterrier") ||
               rawBreed.Contains("bulldog") ||
               compactBreed.Contains("bulldog") ||
               rawBreed.Contains("boxer") ||
               compactBreed.Contains("boxer") ||
               rawBreed.Contains("bully") ||
               compactBreed.Contains("bully") ||
               compactBreed.Contains("bull");
    }

    static bool ContainsShepherdBreedText(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breed);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breed);
        string compactBreed = GetCompactBreedText(breed);

        if (separatorNormalizedBreed.Contains("central asian shepherd") ||
            compactBreed.Contains("centralasianshepherd"))
        {
            return false;
        }

        return separatorNormalizedBreed.Contains("german shepherd") ||
               separatorNormalizedBreed.Contains("german shepard") ||
               separatorNormalizedBreed.Contains("belgian malinois") ||
               separatorNormalizedBreed.Contains("belgian tervuren") ||
               rawBreed.Contains("shepherd") ||
               rawBreed.Contains("shepard") ||
               compactBreed.Contains("shepherd") ||
               compactBreed.Contains("shepard") ||
               rawBreed.Contains("malinois") ||
               compactBreed.Contains("malinois") ||
               rawBreed.Contains("tervuren") ||
               compactBreed.Contains("tervuren") ||
               compactBreed.Contains("german");
    }

    static bool IsShepherdBullyHybridText(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return false;
        }

        string compactBreed = GetCompactBreedText(breed);

        return compactBreed.Contains("germanbull") ||
               compactBreed.Contains("germanbully") ||
               compactBreed.Contains("shepherdbull") ||
               compactBreed.Contains("shepherdbully") ||
               compactBreed.Contains("pitgerman") ||
               compactBreed.Contains("pitshepherd") ||
               compactBreed.Contains("bullshepherd") ||
               compactBreed.Contains("bullyshepherd") ||
               (ContainsShepherdBreedText(breed) && ContainsBullyBreedText(breed) && IsHybridBreedText(breed));
    }

    static bool IsHybridBreedText(string breed)
    {
        if (string.IsNullOrWhiteSpace(breed))
        {
            return false;
        }

        string rawBreed = GetRawNormalizedBreedText(breed);
        string separatorNormalizedBreed = GetSeparatorNormalizedBreedText(breed);
        string compactBreed = GetCompactBreedText(breed);

        return rawBreed.Contains("hybrid") ||
               rawBreed.Contains("mix") ||
               rawBreed.Contains("mixed") ||
               rawBreed.Contains("cross") ||
               separatorNormalizedBreed.Contains(" x ") ||
               compactBreed.Contains("hybrid") ||
               compactBreed.Contains("mixed") ||
               compactBreed.Contains("cross");
    }

    static string GetDogIdentityKey(Dog dog)
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

    static string NormalizeBreedName(string breedName)
    {
        string cleanBreed = string.IsNullOrWhiteSpace(breedName)
            ? "Unknown Breed"
            : breedName.Trim();

        if (string.Equals(cleanBreed, "German Shepard", StringComparison.OrdinalIgnoreCase))
        {
            return "German Shepherd";
        }

        return cleanBreed;
    }

    static string GetRawNormalizedBreedText(string breed)
    {
        return string.IsNullOrWhiteSpace(breed)
            ? string.Empty
            : breed.Trim().ToLowerInvariant();
    }

    static string GetSeparatorNormalizedBreedText(string breed)
    {
        string rawBreed = GetRawNormalizedBreedText(breed);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(rawBreed.Length);
        bool previousWasSpace = false;

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
                if (!previousWasSpace)
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

    static string GetCompactBreedText(string breed)
    {
        string rawBreed = GetRawNormalizedBreedText(breed);

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

    static int GetDeterministicIndex(string seed, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        uint hash = 2166136261;
        string safeSeed = seed ?? string.Empty;

        for (int i = 0; i < safeSeed.Length; i++)
        {
            hash ^= safeSeed[i];
            hash *= 16777619;
        }

        return (int)(hash % count);
    }
}
