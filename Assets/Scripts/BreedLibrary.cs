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
    private static readonly List<BreedRootDefinition> breedRootDefinitions = BuildBreedRootDefinitions();

    public static bool TryGetBaseBreed(string breedName, out BreedInfo breedInfo)
    {
        return baseBreeds.TryGetValue(CleanBreedName(breedName), out breedInfo);
    }

    public static List<string> GetBaseBreedNames()
    {
        return new List<string>(baseBreeds.Keys);
    }

    public static string GetHybridBreedName(string parentBreed1, string parentBreed2)
    {
        string breed1 = CleanBreedName(parentBreed1);
        string breed2 = CleanBreedName(parentBreed2);

        if (string.Equals(breed1, breed2, StringComparison.OrdinalIgnoreCase) &&
            !LooksHybridLikeBreedName(breed1))
        {
            return GetBreedWords(breed1).Length <= 3
                ? CollapseSpaces(breed1)
                : FinalizeBreedNameFromRoots(ExtractBreedRoots(breed1));
        }

        string pairKey = MakePairKey(breed1, breed2);

        if (hybridNames.TryGetValue(pairKey, out string hybridName))
        {
            return CollapseSpaces(hybridName);
        }

        return BuildRootHybridBreedName(breed1, breed2);
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

        AddHybridName(names, "Pit Bull", "Rottweiler", "Pit Rott");
        AddHybridName(names, "German Shepherd", "Rottweiler", "Shepherd Rott");
        AddHybridName(names, "Cane Corso", "Pit Bull", "Corso Pit");
        AddHybridName(names, "Belgian Malinois", "German Shepherd", "Malinois Shepherd");
        AddHybridName(names, "Rottweiler", "Doberman", "Rotterman");
        AddHybridName(names, "Boxer", "Mastiff", "Boxer Mastiff");
        AddHybridName(names, "Pit Bull", "Dogo Argentino", "Pit Dogo");
        AddHybridName(names, "Presa Canario", "Cane Corso", "Corso Presa");
        AddHybridName(names, "Akita", "Cane Corso", "Corso Akita");
        AddHybridName(names, "Greyhound", "Doberman", "Doberman Greyhound");
        AddHybridName(names, "German Shepherd", "Dogo Argentino", "Dogo Shepherd");
        AddHybridName(names, "Pit Bull", "Akita", "Pit Akita");
        AddHybridName(names, "Cane Corso", "Rottweiler", "Corso Rott");
        AddHybridName(names, "Mastiff", "Rottweiler", "Rottmastiff");
        AddHybridName(names, "Presa Canario", "Pit Bull", "Pit Presa");
        AddHybridName(names, "Doberman", "Cane Corso", "Corso Doberman");
        AddHybridName(names, "Boxer", "Rottweiler", "Boxer Rott");
        AddHybridName(names, "German Shepherd", "Mastiff", "Mastiff Shepherd");
        AddHybridName(names, "Belgian Malinois", "Doberman", "Doberman Malinois");
        AddHybridName(names, "German Shepherd", "Pit Bull", "Pit Bullshep");
        AddHybridName(names, "Shepherd", "Pit Bull", "Pit Bullshep");
        AddHybridName(names, "German Shepherd", "American Bully", "Bullyshep");
        AddHybridName(names, "Shepherd", "American Bully", "Bullyshep");
        AddHybridName(names, "Belgian Malinois", "Pit Bull", "Pit Malinois");
        AddHybridName(names, "Akita", "Husky", "Husky Akita");
        AddHybridName(names, "Greyhound", "Pit Bull", "Pit Bullhound");
        AddHybridName(names, "Wolfdog", "German Shepherd", "Wolfdog Shepherd");
        AddHybridName(names, "Wolfdog", "Husky", "Wolfdog Husky");
        AddHybridName(names, "Alaskan Mastiff", "Rottweiler", "Alaskan Rottmastiff");
        AddHybridName(names, "AlaskanMastiffRottweiler", "Black Inu", "Rottmastiff Inu");
        AddHybridName(names, "Rott Mastiff Hybrid", "Black Inu", "Rottmastiff Inu");
        AddHybridName(names, "Pit Bull", "Boxer", "Pit Boxer");
        AddHybridName(names, "Greyhound", "Husky", "Husky Greyhound");
        AddHybridName(names, "German Shepherd", "Husky", "Shepsky");
        AddHybridName(names, "Shepherd", "Husky", "Shepsky");
        AddHybridName(names, "Bulldog", "German Shepherd", "Bullshep");
        AddHybridName(names, "Bulldog", "Shepherd", "Bullshep");
        AddHybridName(names, "Bulldog Shepherd", "Catahoula", "Bullshep Catahoula");
        AddHybridName(names, "Pit Bull Shepherd", "Catahoula", "Pit Bullshep Catahoula");
        AddHybridName(names, "Bulldog", "Catahoula", "Catabull");
        AddHybridName(names, "Bulldog", "Greyhound", "Bullhound");
        AddHybridName(names, "American Bully", "Greyhound", "Bullyhound");
        AddHybridName(names, "Bully", "Greyhound", "Bullyhound");
        AddHybridName(names, "Rott Mastiff", "Black Inu", "Rottmastiff Inu");
        AddHybridName(names, "Catahoula Bulldog", "Shepherd", "Catabull Shep");
        AddHybridName(names, "Catahoula Bulldog", "German Shepherd", "Catabull Shep");
        AddHybridName(names, "Bulldog Shepherd Catahoula", "Rottweiler", "Bullshep Rott");

        return names;
    }

    private static void AddHybridName(
        Dictionary<string, string> names,
        string breed1,
        string breed2,
        string hybridName)
    {
        names[MakePairKey(breed1, breed2)] = CollapseSpaces(hybridName);
    }

    private static List<BreedRootDefinition> BuildBreedRootDefinitions()
    {
        List<BreedRootDefinition> roots = new List<BreedRootDefinition>();

        roots.Add(new BreedRootDefinition("Black Inu", 1, "black inu", "blackinu"));
        roots.Add(new BreedRootDefinition("Shiba Inu", 2, "shiba inu", "shibainu"));
        roots.Add(new BreedRootDefinition("Alaskan", 3, "alaskan mastiff", "alaskanmastiff", "alaskan malamute", "alaskanmalamute", "alaskan"));

        roots.Add(new BreedRootDefinition("Rott", 10, "rottweiler", "rott"));
        roots.Add(new BreedRootDefinition("Mastiff", 11, "mastiff", "english mastiff", "tibetan mastiff"));
        roots.Add(new BreedRootDefinition("Corso", 12, "cane corso", "canecorso", "corso"));
        roots.Add(new BreedRootDefinition("Pit", 13, "pit bull", "pitbull"));
        roots.Add(new BreedRootDefinition("Bully", 14, "american bully", "americanbully", "bully"));
        roots.Add(new BreedRootDefinition("Staffy", 15, "american staffordshire terrier", "americanstaffordshireterrier", "staffordshire bull terrier", "staffordshirebullterrier", "staffordshire"));
        roots.Add(new BreedRootDefinition("Presa", 16, "presa canario", "presacanario", "presa"));
        roots.Add(new BreedRootDefinition("Dogo", 17, "dogo argentino", "dogoargentino", "dogo"));
        roots.Add(new BreedRootDefinition("Boerboel", 18, "boerboel"));
        roots.Add(new BreedRootDefinition("Dane", 19, "great dane", "greatdane", "dane"));
        roots.Add(new BreedRootDefinition("Doberman", 20, "doberman"));
        roots.Add(new BreedRootDefinition("Boxer", 21, "boxer"));
        roots.Add(new BreedRootDefinition("Bulldog", 22, "bulldog"));
        roots.Add(new BreedRootDefinition("Bull", 23, "bull terrier", "bullterrier", "bull"));
        roots.Add(new BreedRootDefinition("Akita", 24, "akita"));
        roots.Add(new BreedRootDefinition("Kangal", 25, "kangal"));
        roots.Add(new BreedRootDefinition("Tosa", 26, "tosa inu", "tosainu", "tosa"));
        roots.Add(new BreedRootDefinition("Fila", 27, "fila brasileiro", "filabrasileiro", "fila"));

        roots.Add(new BreedRootDefinition("Shepherd", 40, "german shepherd", "germanshepherd", "german shepard", "germanshepard", "shepherd", "shepard"));
        roots.Add(new BreedRootDefinition("Malinois", 41, "belgian malinois", "belgianmalinois", "malinois"));
        roots.Add(new BreedRootDefinition("Dutch", 42, "dutch shepherd", "dutchshepherd", "dutch"));
        roots.Add(new BreedRootDefinition("Anatolian", 43, "anatolian shepherd", "anatolianshepherd", "anatolian"));
        roots.Add(new BreedRootDefinition("Australian", 44, "australian shepherd", "australianshepherd", "australian"));
        roots.Add(new BreedRootDefinition("Husky", 45, "siberian husky", "siberianhusky", "husky"));
        roots.Add(new BreedRootDefinition("Malamute", 46, "alaskan malamute", "alaskanmalamute", "malamute"));
        roots.Add(new BreedRootDefinition("Inu", 47, "inu"));
        roots.Add(new BreedRootDefinition("Chow", 48, "chow chow", "chowchow", "chow"));
        roots.Add(new BreedRootDefinition("Samoyed", 49, "samoyed"));
        roots.Add(new BreedRootDefinition("Wolfdog", 50, "wolfdog"));

        roots.Add(new BreedRootDefinition("Greyhound", 70, "greyhound"));
        roots.Add(new BreedRootDefinition("Whippet", 71, "whippet"));
        roots.Add(new BreedRootDefinition("Saluki", 72, "saluki"));
        roots.Add(new BreedRootDefinition("Ridgeback", 73, "rhodesian ridgeback", "rhodesianridgeback", "thai ridgeback", "thairidgeback", "ridgeback"));
        roots.Add(new BreedRootDefinition("Pharaoh", 74, "pharaoh hound", "pharaohhound", "pharaoh"));
        roots.Add(new BreedRootDefinition("Catahoula", 75, "catahoula leopard dog", "catahoulaleoparddog", "catahoula"));
        roots.Add(new BreedRootDefinition("Hound", 76, "hound"));

        return roots;
    }

    private static string BuildRootHybridBreedName(string breed1, string breed2)
    {
        List<string> parent1Roots = ExtractBreedRoots(breed1);
        List<string> parent2Roots = ExtractBreedRoots(breed2);
        List<string> combinedRoots = MergeBreedRoots(parent1Roots, parent2Roots);

        return BuildCompressedHybridBreedName(parent1Roots, parent2Roots, combinedRoots);
    }

    private static string BuildCompressedHybridBreedName(
        List<string> parent1Roots,
        List<string> parent2Roots,
        List<string> combinedRoots)
    {
        List<string> displayRoots = SimplifyRootsForOverflow(combinedRoots);

        if (displayRoots.Count == 0)
        {
            return "Mixed Breed";
        }

        string specialName = GetSpecialHybridDisplayName(parent1Roots, parent2Roots, displayRoots);

        if (!string.IsNullOrWhiteSpace(specialName))
        {
            return CollapseSpaces(specialName);
        }

        List<string> workingRoots = new List<string>(displayRoots);
        List<string> displayParts = new List<string>();

        AddCompressedDisplayPart(workingRoots, displayParts);
        AddRemainingDisplayParts(workingRoots, displayParts);

        return CollapseSpaces(string.Join(" ", displayParts.ToArray()));
    }

    private static string GetSpecialHybridDisplayName(
        List<string> parent1Roots,
        List<string> parent2Roots,
        List<string> displayRoots)
    {
        List<string> simplifiedParent1Roots = SimplifyRootsForOverflow(parent1Roots);
        List<string> simplifiedParent2Roots = SimplifyRootsForOverflow(parent2Roots);

        if (ContainsRoot(displayRoots, "Rott") &&
            ContainsRoot(displayRoots, "Mastiff") &&
            ContainsRoot(displayRoots, "Inu"))
        {
            return "Rottmastiff Inu";
        }

        if (ContainsRoot(displayRoots, "Alaskan") &&
            ContainsRoot(displayRoots, "Rott") &&
            ContainsRoot(displayRoots, "Mastiff"))
        {
            return "Alaskan Rottmastiff";
        }

        if (ContainsRoot(displayRoots, "Husky") &&
            ContainsRoot(displayRoots, "Akita") &&
            displayRoots.Count <= 2)
        {
            return "Husky Akita";
        }

        if (ParentHasRoots(simplifiedParent1Roots, "Pit", "Shepherd") ||
            ParentHasRoots(simplifiedParent2Roots, "Pit", "Shepherd"))
        {
            if (ContainsRoot(displayRoots, "Rott"))
            {
                return "Pit Bullshep Rott";
            }

            if (ContainsRoot(displayRoots, "Catahoula"))
            {
                return "Pit Bullshep Catahoula";
            }
        }

        if (ParentHasRoots(simplifiedParent1Roots, "Catahoula", "Bulldog") ||
            ParentHasRoots(simplifiedParent2Roots, "Catahoula", "Bulldog"))
        {
            if (ContainsRoot(displayRoots, "Shepherd"))
            {
                return "Catabull Shep";
            }

            if (ContainsRoot(displayRoots, "Rott"))
            {
                return "Catabull Rott";
            }
        }

        if (ParentHasRoots(simplifiedParent1Roots, "Bulldog", "Shepherd") ||
            ParentHasRoots(simplifiedParent2Roots, "Bulldog", "Shepherd"))
        {
            if (ContainsRoot(displayRoots, "Rott"))
            {
                return "Bullshep Rott";
            }

            if (ContainsRoot(displayRoots, "Catahoula"))
            {
                return "Bullshep Catahoula";
            }
        }

        if (ParentHasRoots(simplifiedParent1Roots, "Rott", "Mastiff") ||
            ParentHasRoots(simplifiedParent2Roots, "Rott", "Mastiff"))
        {
            if (ContainsRoot(displayRoots, "Inu"))
            {
                return "Rottmastiff Inu";
            }

            if (ContainsRoot(displayRoots, "Alaskan"))
            {
                return "Alaskan Rottmastiff";
            }
        }

        return string.Empty;
    }

    private static bool ParentHasRoots(List<string> parentRoots, string root1, string root2)
    {
        return ContainsRoot(parentRoots, root1) && ContainsRoot(parentRoots, root2);
    }

    private static void AddCompressedDisplayPart(List<string> workingRoots, List<string> displayParts)
    {
        if (TryUseCompressedPart(workingRoots, displayParts, "Pit", "Shepherd", "Pit Bullshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bully", "Shepherd", "Bullyshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Staffy", "Shepherd", "Staffshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Boxer", "Shepherd", "Boxshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bulldog", "Shepherd", "Bullshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Catahoula", "Bulldog", "Catabull") ||
            TryUseCompressedPart(workingRoots, displayParts, "Pit", "Catahoula", "Pit Catahoula") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bully", "Catahoula", "Bullyhoula") ||
            TryUseCompressedPart(workingRoots, displayParts, "Shepherd", "Catahoula", "Catahoula Shep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Pit", "Greyhound", "Pit Bullhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bulldog", "Greyhound", "Bullhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bully", "Greyhound", "Bullyhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Boxer", "Greyhound", "Boxerhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Staffy", "Greyhound", "Staffhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Rott", "Greyhound", "Rotthound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Mastiff", "Greyhound", "Mastihound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Shepherd", "Greyhound", "Shephound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Husky", "Greyhound", "Huskyhound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Akita", "Greyhound", "Akitahound") ||
            TryUseCompressedPart(workingRoots, displayParts, "Rott", "Shepherd", "Rottshep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Mastiff", "Shepherd", "Mastishep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Husky", "Shepherd", "Shepsky") ||
            TryUseCompressedPart(workingRoots, displayParts, "Akita", "Shepherd", "Akita Shep") ||
            TryUseCompressedPart(workingRoots, displayParts, "Rott", "Mastiff", "Rottmastiff") ||
            TryUseCompressedPart(workingRoots, displayParts, "Corso", "Rott", "Corso Rott") ||
            TryUseCompressedPart(workingRoots, displayParts, "Rott", "Doberman", "Rotterman") ||
            TryUseCompressedPart(workingRoots, displayParts, "Pit", "Bulldog", "Pit Bulldog") ||
            TryUseCompressedPart(workingRoots, displayParts, "Bully", "Bulldog", "Bully Bulldog"))
        {
            return;
        }
    }

    private static bool TryUseCompressedPart(
        List<string> workingRoots,
        List<string> displayParts,
        string root1,
        string root2,
        string displayPart)
    {
        if (!ContainsRoot(workingRoots, root1) || !ContainsRoot(workingRoots, root2))
        {
            return false;
        }

        RemoveRoot(workingRoots, root1);
        RemoveRoot(workingRoots, root2);
        displayParts.Add(displayPart);
        return true;
    }

    private static void AddRemainingDisplayParts(List<string> workingRoots, List<string> displayParts)
    {
        List<string> sortedRoots = SortBreedRoots(workingRoots);

        for (int i = 0; i < sortedRoots.Count && displayParts.Count < 2; i++)
        {
            displayParts.Add(GetDisplayRootName(sortedRoots[i], displayParts.Count > 0));
        }

        if (displayParts.Count == 0)
        {
            displayParts.Add("Mixed Breed");
        }
    }

    private static string GetDisplayRootName(string root, bool isAfterCompressedPart)
    {
        if (isAfterCompressedPart && string.Equals(root, "Shepherd", StringComparison.OrdinalIgnoreCase))
        {
            return "Shep";
        }

        return root;
    }

    private static List<string> ExtractBreedRoots(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string separatedBreed = GetSeparatedBreedText(breedName);
        string compactBreed = GetCompactBreedText(breedName);
        List<string> roots = new List<string>();

        for (int i = 0; i < breedRootDefinitions.Count; i++)
        {
            BreedRootDefinition rootDefinition = breedRootDefinitions[i];

            if (RootMatches(rootDefinition, rawBreed, separatedBreed, compactBreed))
            {
                AddRootIfMissing(roots, rootDefinition.root);
            }
        }

        AddRootsFromKnownShorthand(roots, compactBreed);
        NormalizeRootConflicts(roots);
        return SortBreedRoots(roots);
    }

    private static void AddRootsFromKnownShorthand(List<string> roots, string compactBreed)
    {
        if (string.IsNullOrWhiteSpace(compactBreed))
        {
            return;
        }

        bool hasPitBullshep = compactBreed.Contains("pitbullshep");
        bool hasPitBullhound = compactBreed.Contains("pitbullhound");

        if (hasPitBullshep)
        {
            AddRootIfMissing(roots, "Pit");
            AddRootIfMissing(roots, "Shepherd");
        }

        if (!hasPitBullshep && compactBreed.Contains("bullshep"))
        {
            AddRootIfMissing(roots, "Bulldog");
            AddRootIfMissing(roots, "Shepherd");
        }

        AddShorthandRootsIfMatched(roots, compactBreed, "bullyshep", "Bully", "Shepherd");
        AddShorthandRootsIfMatched(roots, compactBreed, "staffshep", "Staffy", "Shepherd");
        AddShorthandRootsIfMatched(roots, compactBreed, "boxshep", "Boxer", "Shepherd");
        AddShorthandRootsIfMatched(roots, compactBreed, "catabull", "Catahoula", "Bulldog");
        AddShorthandRootsIfMatched(roots, compactBreed, "rottshep", "Rott", "Shepherd");
        AddShorthandRootsIfMatched(roots, compactBreed, "mastishep", "Mastiff", "Shepherd");
        AddShorthandRootsIfMatched(roots, compactBreed, "shepsky", "Shepherd", "Husky");
        AddShorthandRootsIfMatched(roots, compactBreed, "rottmastiff", "Rott", "Mastiff");
        AddShorthandRootsIfMatched(roots, compactBreed, "rotterman", "Rott", "Doberman");

        if (hasPitBullhound)
        {
            AddRootIfMissing(roots, "Pit");
            AddRootIfMissing(roots, "Greyhound");
        }

        if (!hasPitBullhound && compactBreed.Contains("bullhound"))
        {
            AddRootIfMissing(roots, "Bulldog");
            AddRootIfMissing(roots, "Greyhound");
        }

        AddShorthandRootsIfMatched(roots, compactBreed, "bullyhound", "Bully", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "boxerhound", "Boxer", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "staffhound", "Staffy", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "rotthound", "Rott", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "mastihound", "Mastiff", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "shephound", "Shepherd", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "huskyhound", "Husky", "Greyhound");
        AddShorthandRootsIfMatched(roots, compactBreed, "akitahound", "Akita", "Greyhound");
    }

    private static void AddShorthandRootsIfMatched(
        List<string> roots,
        string compactBreed,
        string shorthand,
        string root1,
        string root2)
    {
        if (!compactBreed.Contains(shorthand))
        {
            return;
        }

        AddRootIfMissing(roots, root1);
        AddRootIfMissing(roots, root2);
    }

    private static bool RootMatches(
        BreedRootDefinition rootDefinition,
        string rawBreed,
        string separatedBreed,
        string compactBreed)
    {
        for (int i = 0; i < rootDefinition.patterns.Length; i++)
        {
            string pattern = rootDefinition.patterns[i];
            string rawPattern = GetRawBreedText(pattern);
            string separatedPattern = GetSeparatedBreedText(pattern);
            string compactPattern = GetCompactBreedText(pattern);

            if ((!string.IsNullOrEmpty(rawPattern) && rawBreed.Contains(rawPattern)) ||
                (!string.IsNullOrEmpty(separatedPattern) && separatedBreed.Contains(separatedPattern)) ||
                (!string.IsNullOrEmpty(compactPattern) && compactBreed.Contains(compactPattern)))
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> MergeBreedRoots(List<string> parent1Roots, List<string> parent2Roots)
    {
        List<string> mergedRoots = new List<string>();

        AddRootsIfMissing(mergedRoots, parent1Roots);
        AddRootsIfMissing(mergedRoots, parent2Roots);

        return SortBreedRoots(mergedRoots);
    }

    private static List<string> CapBreedRoots(
        List<string> parent1Roots,
        List<string> parent2Roots,
        List<string> combinedRoots)
    {
        if (combinedRoots.Count <= 3)
        {
            return SortBreedRoots(combinedRoots);
        }

        List<string> cappedRoots = new List<string>();
        AddRootIfMissing(cappedRoots, GetPreferredRootForCap(parent1Roots));
        AddRootIfMissing(cappedRoots, GetPreferredRootForCap(parent2Roots));

        List<string> simplifiedCombinedRoots = SimplifyRootsForOverflow(combinedRoots);

        for (int i = 0; i < simplifiedCombinedRoots.Count && cappedRoots.Count < 3; i++)
        {
            AddRootIfMissing(cappedRoots, simplifiedCombinedRoots[i]);
        }

        return SortBreedRoots(cappedRoots);
    }

    private static string GetPreferredRootForCap(List<string> roots)
    {
        List<string> simplifiedRoots = SimplifyRootsForOverflow(roots);

        if (simplifiedRoots.Count == 0)
        {
            return string.Empty;
        }

        return SortBreedRoots(simplifiedRoots)[0];
    }

    private static List<string> SimplifyRootsForOverflow(List<string> roots)
    {
        List<string> simplifiedRoots = new List<string>();

        for (int i = 0; i < roots.Count; i++)
        {
            AddRootIfMissing(simplifiedRoots, SimplifyRootForOverflow(roots[i]));
        }

        NormalizeRootConflicts(simplifiedRoots);
        return SortBreedRoots(simplifiedRoots);
    }

    private static string SimplifyRootForOverflow(string root)
    {
        if (string.Equals(root, "Black Inu", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(root, "Shiba Inu", StringComparison.OrdinalIgnoreCase))
        {
            return "Inu";
        }

        return root;
    }

    private static string FinalizeBreedNameFromRoots(List<string> roots)
    {
        List<string> finalRoots = SortBreedRoots(roots);

        if (finalRoots.Count == 0)
        {
            return "Mixed Breed";
        }

        if (finalRoots.Count > 3)
        {
            finalRoots = CapBreedRoots(new List<string>(), new List<string>(), finalRoots);
        }

        return string.Join(" ", finalRoots.ToArray());
    }

    private static bool LooksHybridLikeBreedName(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        List<string> roots = ExtractBreedRoots(breedName);

        return roots.Count > 1 ||
               rawBreed.Contains("hybrid") ||
               rawBreed.Contains("mix") ||
               rawBreed.Contains("mixed") ||
               rawBreed.Contains("cross");
    }

    private static void AddRootsIfMissing(List<string> targetRoots, List<string> sourceRoots)
    {
        if (sourceRoots == null)
        {
            return;
        }

        for (int i = 0; i < sourceRoots.Count; i++)
        {
            AddRootIfMissing(targetRoots, sourceRoots[i]);
        }
    }

    private static void AddRootIfMissing(List<string> roots, string root)
    {
        if (roots == null || string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        for (int i = 0; i < roots.Count; i++)
        {
            if (string.Equals(roots[i], root, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        roots.Add(root);
    }

    private static void NormalizeRootConflicts(List<string> roots)
    {
        if (roots == null)
        {
            return;
        }

        if (ContainsRoot(roots, "Pit") ||
            ContainsRoot(roots, "Bully") ||
            ContainsRoot(roots, "Staffy") ||
            ContainsRoot(roots, "Bulldog"))
        {
            RemoveRoot(roots, "Bull");
        }

        if (ContainsRoot(roots, "Black Inu") || ContainsRoot(roots, "Shiba Inu"))
        {
            RemoveRoot(roots, "Inu");
        }

        if (ContainsRoot(roots, "Greyhound") ||
            ContainsRoot(roots, "Whippet") ||
            ContainsRoot(roots, "Saluki") ||
            ContainsRoot(roots, "Ridgeback") ||
            ContainsRoot(roots, "Pharaoh"))
        {
            RemoveRoot(roots, "Hound");
        }

        if (ContainsRoot(roots, "Dutch") ||
            ContainsRoot(roots, "Anatolian") ||
            ContainsRoot(roots, "Australian"))
        {
            RemoveRoot(roots, "Shepherd");
        }

        if (ContainsRoot(roots, "Alaskan") && ContainsRoot(roots, "Malamute"))
        {
            RemoveRoot(roots, "Alaskan");
        }
    }

    private static bool ContainsRoot(List<string> roots, string root)
    {
        if (roots == null)
        {
            return false;
        }

        for (int i = 0; i < roots.Count; i++)
        {
            if (string.Equals(roots[i], root, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveRoot(List<string> roots, string root)
    {
        if (roots == null)
        {
            return;
        }

        for (int i = roots.Count - 1; i >= 0; i--)
        {
            if (string.Equals(roots[i], root, StringComparison.OrdinalIgnoreCase))
            {
                roots.RemoveAt(i);
            }
        }
    }

    private static List<string> SortBreedRoots(List<string> roots)
    {
        List<string> sortedRoots = new List<string>();

        if (roots == null)
        {
            return sortedRoots;
        }

        AddRootsByOrder(sortedRoots, roots);
        return sortedRoots;
    }

    private static void AddRootsByOrder(List<string> sortedRoots, List<string> roots)
    {
        for (int i = 0; i < breedRootDefinitions.Count; i++)
        {
            string root = breedRootDefinitions[i].root;

            if (ContainsRoot(roots, root))
            {
                AddRootIfMissing(sortedRoots, root);
            }
        }

        for (int i = 0; i < roots.Count; i++)
        {
            AddRootIfMissing(sortedRoots, roots[i]);
        }
    }

    private class BreedRootDefinition
    {
        public readonly string root;
        public readonly int sortOrder;
        public readonly string[] patterns;

        public BreedRootDefinition(string root, int sortOrder, params string[] patterns)
        {
            this.root = root;
            this.sortOrder = sortOrder;
            this.patterns = patterns ?? new string[0];
        }
    }

    private static string GetRawBreedText(string breedName)
    {
        return string.IsNullOrWhiteSpace(breedName)
            ? string.Empty
            : breedName.Trim().ToLowerInvariant();
    }

    private static string GetSeparatedBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        List<char> characters = new List<char>();
        bool previousWasSpace = false;

        for (int i = 0; i < rawBreed.Length; i++)
        {
            char breedCharacter = rawBreed[i];
            bool isSeparator = char.IsWhiteSpace(breedCharacter) ||
                               breedCharacter == '_' ||
                               breedCharacter == '-' ||
                               breedCharacter == '/' ||
                               breedCharacter == '\\' ||
                               breedCharacter == '\'';

            if (isSeparator)
            {
                if (!previousWasSpace && characters.Count > 0)
                {
                    characters.Add(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            characters.Add(breedCharacter);
            previousWasSpace = false;
        }

        return new string(characters.ToArray()).Trim();
    }

    private static string GetCompactBreedText(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);

        if (string.IsNullOrEmpty(rawBreed))
        {
            return string.Empty;
        }

        List<char> characters = new List<char>();

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

            characters.Add(breedCharacter);
        }

        return new string(characters.ToArray());
    }

    private static string CollapseSpaces(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string[] parts = value.Trim().Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries);

        return string.Join(" ", parts);
    }

    private static string[] GetBreedWords(string breedName)
    {
        return CleanBreedName(breedName).Split(
            new[] { ' ', '-', '/', '_', '\\', '\'' },
            StringSplitOptions.RemoveEmptyEntries);
    }
}
