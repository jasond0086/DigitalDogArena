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
        public int intelligenceBias;
        public FightStyle styleTendency;
        public string visualDescription;

        public BreedInfo(
            string displayName,
            int strengthBias,
            int agilityBias,
            int staminaBias,
            int intelligenceBias,
            FightStyle styleTendency,
            string visualDescription)
        {
            this.displayName = displayName;
            this.strengthBias = strengthBias;
            this.agilityBias = agilityBias;
            this.staminaBias = staminaBias;
            this.intelligenceBias = intelligenceBias;
            this.styleTendency = styleTendency;
            this.visualDescription = visualDescription;
        }
    }

    private static readonly Dictionary<string, BreedInfo> baseBreeds =
        new Dictionary<string, BreedInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "Pit Bull", new BreedInfo("Pit Bull", 3, 1, 2, 1, FightStyle.Rushdown, "Compact, muscular, and explosive.") },
            { "Rottweiler", new BreedInfo("Rottweiler", 3, 0, 3, 1, FightStyle.Tank, "Heavy frame, broad chest, and steady pressure.") },
            { "Cane Corso", new BreedInfo("Cane Corso", 3, 0, 2, 0, FightStyle.Tank, "Powerful mastiff build with a hard guard-dog presence.") },
            { "Presa Canario", new BreedInfo("Presa Canario", 3, 0, 2, 0, FightStyle.Balanced, "Large, rugged, and imposing.") },
            { "Dogo Argentino", new BreedInfo("Dogo Argentino", 2, 2, 2, 1, FightStyle.Balanced, "Athletic white-coated hunter build.") },
            { "Mastiff", new BreedInfo("Mastiff", 4, -1, 3, -1, FightStyle.Tank, "Massive body, heavy bone, and crushing weight.") },
            { "German Shepherd", new BreedInfo("German Shepherd", 1, 2, 2, 4, FightStyle.Counter, "Lean working-dog shape with alert posture.") },
            { "Belgian Malinois", new BreedInfo("Belgian Malinois", 0, 4, 2, 4, FightStyle.Rushdown, "Light, fast, and sharply athletic.") },
            { "Doberman", new BreedInfo("Doberman", 1, 3, 1, 3, FightStyle.Counter, "Sleek, tall, and quick-footed.") },
            { "Boxer", new BreedInfo("Boxer", 2, 2, 1, 1, FightStyle.Wildcard, "Square, springy, and brawler-built.") },
            { "Akita", new BreedInfo("Akita", 2, 1, 3, 2, FightStyle.Balanced, "Thick-coated, sturdy, and stubborn.") },
            { "Greyhound", new BreedInfo("Greyhound", -1, 5, 1, 1, FightStyle.Rushdown, "Long-legged, narrow, and built for speed.") },
            { "American Bully", new BreedInfo("American Bully", 4, 0, 2, 1, FightStyle.Rushdown, "Broad bully frame with short-burst power.") },
            { "American Staffordshire Terrier", new BreedInfo("American Staffordshire Terrier", 3, 2, 2, 1, FightStyle.Rushdown, "Compact terrier muscle with sharp forward drive.") },
            { "Staffordshire Bull Terrier", new BreedInfo("Staffordshire Bull Terrier", 3, 2, 1, 1, FightStyle.Rushdown, "Small, dense, and explosive in close quarters.") },
            { "Bull Terrier", new BreedInfo("Bull Terrier", 2, 2, 1, 1, FightStyle.Wildcard, "Low, sturdy, and unpredictable with terrier snap.") },
            { "Bulldog", new BreedInfo("Bulldog", 3, -1, 2, 0, FightStyle.Tank, "Low heavy stance with stubborn pressure.") },
            { "Dutch Shepherd", new BreedInfo("Dutch Shepherd", 1, 3, 2, 4, FightStyle.Counter, "Agile working-dog frame with quick reads.") },
            { "Anatolian Shepherd", new BreedInfo("Anatolian Shepherd", 3, 0, 4, 2, FightStyle.Tank, "Large guardian build with patient endurance.") },
            { "Belgian Tervuren", new BreedInfo("Belgian Tervuren", 0, 3, 2, 4, FightStyle.Counter, "Elegant shepherd build with fast tactical movement.") },
            { "Australian Shepherd", new BreedInfo("Australian Shepherd", 0, 3, 2, 4, FightStyle.Counter, "Smart herding-dog frame with agile footwork.") },
            { "Boerboel", new BreedInfo("Boerboel", 4, -1, 4, 0, FightStyle.Tank, "Dense mastiff power with a crushing guard posture.") },
            { "English Mastiff", new BreedInfo("English Mastiff", 4, -2, 4, -1, FightStyle.Tank, "Huge heavy-bone frame with slow overwhelming force.") },
            { "Tibetan Mastiff", new BreedInfo("Tibetan Mastiff", 4, -1, 4, 1, FightStyle.Tank, "Massive coated guardian with rugged stamina.") },
            { "Great Dane", new BreedInfo("Great Dane", 3, 1, 3, 1, FightStyle.Balanced, "Tall giant-breed reach with steady momentum.") },
            { "Kangal", new BreedInfo("Kangal", 4, 0, 4, 1, FightStyle.Tank, "Powerful livestock guardian with deep endurance.") },
            { "Tosa Inu", new BreedInfo("Tosa Inu", 4, 0, 3, 0, FightStyle.Tank, "Large fighting-dog frame with deliberate pressure.") },
            { "Fila Brasileiro", new BreedInfo("Fila Brasileiro", 4, 0, 3, 0, FightStyle.Tank, "Heavy guardian body with intimidating drive.") },
            { "Central Asian Shepherd", new BreedInfo("Central Asian Shepherd", 4, 0, 4, 1, FightStyle.Tank, "Rugged guardian build with old-world toughness.") },
            { "Beauceron", new BreedInfo("Beauceron", 2, 2, 3, 3, FightStyle.Counter, "Strong working-dog frame with composed reactions.") },
            { "Black Russian Terrier", new BreedInfo("Black Russian Terrier", 3, 1, 3, 2, FightStyle.Tank, "Large black-coated guardian with armored presence.") },
            { "Husky", new BreedInfo("Husky", 1, 3, 4, 2, FightStyle.Balanced, "Light-footed sled build with tireless movement.") },
            { "Alaskan Malamute", new BreedInfo("Alaskan Malamute", 2, 1, 4, 1, FightStyle.Balanced, "Heavy sled-dog body with pulling strength.") },
            { "Shiba Inu", new BreedInfo("Shiba Inu", 1, 3, 2, 2, FightStyle.Counter, "Compact spitz frame with sharp evasive timing.") },
            { "Chow Chow", new BreedInfo("Chow Chow", 2, 0, 3, 1, FightStyle.Balanced, "Stocky spitz guardian with dense-coated presence.") },
            { "Samoyed", new BreedInfo("Samoyed", 1, 2, 3, 2, FightStyle.Balanced, "Bright sled-dog frame with resilient stamina.") },
            { "Whippet", new BreedInfo("Whippet", -1, 5, 1, 1, FightStyle.Rushdown, "Small sighthound body built for sudden speed.") },
            { "Saluki", new BreedInfo("Saluki", -1, 5, 2, 2, FightStyle.Counter, "Lean desert hound with long, evasive strides.") },
            { "Rhodesian Ridgeback", new BreedInfo("Rhodesian Ridgeback", 2, 3, 3, 2, FightStyle.Counter, "Athletic hound build with hunter endurance.") },
            { "Pharaoh Hound", new BreedInfo("Pharaoh Hound", 0, 4, 2, 2, FightStyle.Rushdown, "Lean, springy hound with quick striking movement.") },
            { "Thai Ridgeback", new BreedInfo("Thai Ridgeback", 1, 3, 2, 2, FightStyle.Wildcard, "Rare ridgeback build with agile, unusual angles.") },
            { "Catahoula Leopard Dog", new BreedInfo("Catahoula Leopard Dog", 1, 3, 3, 3, FightStyle.Wildcard, "Versatile working hound with chaotic field instincts.") },
            { "Wolfdog", new BreedInfo("Wolfdog", 2, 3, 4, 3, FightStyle.Wildcard, "Rare wildline frame with endurance and unpredictable movement.") }
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

        AddHybridName(names, "Pit Bull", "Rottweiler", "Rottie Bull Hybrid");
        AddHybridName(names, "German Shepherd", "Rottweiler", "Shepherd Rott Hybrid");
        AddHybridName(names, "Cane Corso", "Pit Bull", "Corso Bull Hybrid");
        AddHybridName(names, "Belgian Malinois", "German Shepherd", "Malishep");
        AddHybridName(names, "Rottweiler", "Doberman", "Doberrott Hybrid");
        AddHybridName(names, "Boxer", "Mastiff", "Boxiff");
        AddHybridName(names, "Pit Bull", "Dogo Argentino", "Bullentino");
        AddHybridName(names, "Presa Canario", "Cane Corso", "Presacorso");
        AddHybridName(names, "Akita", "Cane Corso", "Akicorso");
        AddHybridName(names, "Greyhound", "Doberman", "Greyberman");
        AddHybridName(names, "German Shepherd", "Dogo Argentino", "Shepdogo");
        AddHybridName(names, "Pit Bull", "Akita", "Bullakita");
        AddHybridName(names, "Cane Corso", "Rottweiler", "Corso Rott Hybrid");
        AddHybridName(names, "Mastiff", "Rottweiler", "Mastweiler");
        AddHybridName(names, "Presa Canario", "Pit Bull", "Presabull");
        AddHybridName(names, "Doberman", "Cane Corso", "Dobocorso");
        AddHybridName(names, "Boxer", "Rottweiler", "Boxweiler");
        AddHybridName(names, "German Shepherd", "Mastiff", "Shepiff");
        AddHybridName(names, "Belgian Malinois", "Doberman", "Maliberman");
        AddHybridName(names, "German Shepherd", "Pit Bull", "German Bull Hybrid");
        AddHybridName(names, "German Shepherd", "American Bully", "Shepherd Bully Hybrid");
        AddHybridName(names, "Belgian Malinois", "Pit Bull", "Malinois Bull Hybrid");
        AddHybridName(names, "Akita", "Husky", "Akita Husky Hybrid");
        AddHybridName(names, "Greyhound", "Pit Bull", "Bullhound Hybrid");
        AddHybridName(names, "Wolfdog", "German Shepherd", "Wolf Shepherd Hybrid");
        AddHybridName(names, "Wolfdog", "Husky", "Wolf Husky Hybrid");

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
