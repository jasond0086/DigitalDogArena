using System;
using System.Collections.Generic;

public static class BreedLibrary
{
    public class BreedInfo
    {
        public string displayName;
        public int strengthBias;
        public int agilityBias;
        public int staminaBias;
        public FightStyle styleTendency;
        public string visualDescription;

        public BreedInfo(
            string displayName,
            int strengthBias,
            int agilityBias,
            int staminaBias,
            FightStyle styleTendency,
            string visualDescription)
        {
            this.displayName = displayName;
            this.strengthBias = strengthBias;
            this.agilityBias = agilityBias;
            this.staminaBias = staminaBias;
            this.styleTendency = styleTendency;
            this.visualDescription = visualDescription;
        }
    }

    private static readonly Dictionary<string, BreedInfo> baseBreeds =
        new Dictionary<string, BreedInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "Pit Bull", new BreedInfo("Pit Bull", 3, 1, 2, FightStyle.Rushdown, "Compact, muscular, and explosive.") },
            { "Rottweiler", new BreedInfo("Rottweiler", 3, 0, 3, FightStyle.Tank, "Heavy frame, broad chest, and steady pressure.") },
            { "Cane Corso", new BreedInfo("Cane Corso", 3, 0, 2, FightStyle.Tank, "Powerful mastiff build with a hard guard-dog presence.") },
            { "Presa Canario", new BreedInfo("Presa Canario", 3, 0, 2, FightStyle.Balanced, "Large, rugged, and imposing.") },
            { "Dogo Argentino", new BreedInfo("Dogo Argentino", 2, 2, 2, FightStyle.Balanced, "Athletic white-coated hunter build.") },
            { "Mastiff", new BreedInfo("Mastiff", 4, -1, 3, FightStyle.Tank, "Massive body, heavy bone, and crushing weight.") },
            { "German Shepherd", new BreedInfo("German Shepherd", 1, 2, 2, FightStyle.Counter, "Lean working-dog shape with alert posture.") },
            { "Belgian Malinois", new BreedInfo("Belgian Malinois", 0, 4, 2, FightStyle.Rushdown, "Light, fast, and sharply athletic.") },
            { "Doberman", new BreedInfo("Doberman", 1, 3, 1, FightStyle.Counter, "Sleek, tall, and quick-footed.") },
            { "Boxer", new BreedInfo("Boxer", 2, 2, 1, FightStyle.Wildcard, "Square, springy, and brawler-built.") },
            { "Akita", new BreedInfo("Akita", 2, 1, 3, FightStyle.Balanced, "Thick-coated, sturdy, and stubborn.") },
            { "Greyhound", new BreedInfo("Greyhound", -1, 5, 1, FightStyle.Rushdown, "Long-legged, narrow, and built for speed.") }
        };

    private static readonly Dictionary<string, string> hybridNames = BuildHybridNames();

    public static bool TryGetBaseBreed(string breedName, out BreedInfo breedInfo)
    {
        return baseBreeds.TryGetValue(CleanBreedName(breedName), out breedInfo);
    }

    public static string GetHybridBreedName(string parentBreed1, string parentBreed2)
    {
        string breed1 = CleanBreedName(parentBreed1);
        string breed2 = CleanBreedName(parentBreed2);

        if (string.Equals(breed1, breed2, StringComparison.OrdinalIgnoreCase))
        {
            return breed1;
        }

        string pairKey = MakePairKey(breed1, breed2);

        if (hybridNames.TryGetValue(pairKey, out string hybridName))
        {
            return hybridName;
        }

        return BuildFallbackHybridName(breed1, breed2);
    }

    public static string CleanBreedName(string breedName)
    {
        if (string.IsNullOrWhiteSpace(breedName))
        {
            return "Unknown Breed";
        }

        return breedName.Trim();
    }

    public static string NormalizeBreedName(string breedName)
    {
        return CleanBreedName(breedName);
    }

    private static string MakePairKey(string breed1, string breed2)
    {
        string orderedBreed1;
        string orderedBreed2;
        GetOrderedPair(breed1, breed2, out orderedBreed1, out orderedBreed2);

        return $"{orderedBreed1}|{orderedBreed2}";
    }

    private static void GetOrderedPair(
        string breed1,
        string breed2,
        out string orderedBreed1,
        out string orderedBreed2)
    {
        string cleanBreed1 = CleanBreedName(breed1);
        string cleanBreed2 = CleanBreedName(breed2);

        if (string.Compare(cleanBreed1, cleanBreed2, StringComparison.OrdinalIgnoreCase) <= 0)
        {
            orderedBreed1 = cleanBreed1;
            orderedBreed2 = cleanBreed2;
            return;
        }

        orderedBreed1 = cleanBreed2;
        orderedBreed2 = cleanBreed1;
    }

    private static Dictionary<string, string> BuildHybridNames()
    {
        Dictionary<string, string> names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        AddHybridName(names, "Pit Bull", "Rottweiler", "Bullweiler");
        AddHybridName(names, "German Shepherd", "Rottweiler", "Shepweiler");
        AddHybridName(names, "Cane Corso", "Pit Bull", "Corbull");
        AddHybridName(names, "Belgian Malinois", "German Shepherd", "Malishep");
        AddHybridName(names, "Rottweiler", "Doberman", "Rotterman");
        AddHybridName(names, "Boxer", "Mastiff", "Boxiff");
        AddHybridName(names, "Pit Bull", "Dogo Argentino", "Bullentino");
        AddHybridName(names, "Presa Canario", "Cane Corso", "Presacorso");
        AddHybridName(names, "Akita", "Cane Corso", "Akicorso");
        AddHybridName(names, "Greyhound", "Doberman", "Greyberman");
        AddHybridName(names, "German Shepherd", "Dogo Argentino", "Shepdogo");
        AddHybridName(names, "Pit Bull", "Akita", "Bullakita");
        AddHybridName(names, "Cane Corso", "Rottweiler", "Corsweiler");
        AddHybridName(names, "Mastiff", "Rottweiler", "Mastweiler");
        AddHybridName(names, "Presa Canario", "Pit Bull", "Presabull");
        AddHybridName(names, "Doberman", "Cane Corso", "Dobocorso");
        AddHybridName(names, "Boxer", "Rottweiler", "Boxweiler");
        AddHybridName(names, "German Shepherd", "Mastiff", "Shepiff");
        AddHybridName(names, "Belgian Malinois", "Doberman", "Maliberman");

        return names;
    }

    private static void AddHybridName(
        Dictionary<string, string> names,
        string breed1,
        string breed2,
        string hybridName)
    {
        names[MakePairKey(breed1, breed2)] = hybridName;
    }

    private static string BuildFallbackHybridName(string breed1, string breed2)
    {
        string orderedBreed1;
        string orderedBreed2;
        GetOrderedPair(breed1, breed2, out orderedBreed1, out orderedBreed2);

        return $"{GetFirstWord(orderedBreed1)}{GetSecondWordOrFirst(orderedBreed2)}";
    }

    private static string GetFirstWord(string breedName)
    {
        return GetBreedWord(breedName, 0);
    }

    private static string GetSecondWordOrFirst(string breedName)
    {
        string[] words = GetBreedWords(breedName);

        if (words.Length == 0)
        {
            return "Hybrid";
        }

        if (words.Length > 1)
        {
            return words[1];
        }

        return words[0];
    }

    private static string GetBreedWord(string breedName, int wordIndex)
    {
        string[] words = GetBreedWords(breedName);

        if (words.Length == 0)
        {
            return "Hybrid";
        }

        int safeIndex = Math.Min(wordIndex, words.Length - 1);
        return words[safeIndex];
    }

    private static string[] GetBreedWords(string breedName)
    {
        return CleanBreedName(breedName).Split(
            new[] { ' ', '-', '/' },
            StringSplitOptions.RemoveEmptyEntries);
    }
}
