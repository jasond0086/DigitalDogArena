using System;
using UnityEngine;

public static class DogPortraitLibrary
{
    private const string PortraitResourceRoot = "DogPortraits";

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
