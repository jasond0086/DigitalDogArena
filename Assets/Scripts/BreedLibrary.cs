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

    private enum BreedFamily
    {
        Unknown = 0,
        BullyStriker = 1,
        IronRott = 2,
        GuardMastiff = 3,
        ShepherdSentinel = 4,
        SpitzWarden = 5,
        VelocityHound = 6
    }

    private static readonly BreedFamily[] familyOrder =
    {
        BreedFamily.BullyStriker,
        BreedFamily.IronRott,
        BreedFamily.GuardMastiff,
        BreedFamily.ShepherdSentinel,
        BreedFamily.SpitzWarden,
        BreedFamily.VelocityHound
    };

    private static readonly Dictionary<string, string> hybridNames = BuildHybridNames();
    private static readonly Dictionary<BreedFamily, string[]> sameFamilyNames = BuildSameFamilyNames();
    private static readonly Dictionary<string, string[]> familyPairNames = BuildFamilyPairNames();

    private static readonly string[] deepHybridNames =
    {
        "Obsidian Warden Prime",
        "Northjaw Iron Warden",
        "Blackfang Stone Guard",
        "Cinder Wolf Sentinel",
        "Steeljaw Frost Runner",
        "Ashguard Rott Line",
        "Apex Hybrid",
        "Old Blood"
    };

    private static readonly string[] safeFallbackNames =
    {
        "Mixed Line",
        "Old Blood",
        "Apex Hybrid",
        "Warline Hybrid"
    };

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
            !LooksHybridLikeBreedName(breed1) &&
            GetBreedWords(breed1).Length <= 3)
        {
            return FinalizeHybridBreedName(breed1);
        }

        string pairKey = MakePairKey(breed1, breed2);

        if (hybridNames.TryGetValue(pairKey, out string hybridName))
        {
            return FinalizeHybridBreedName(hybridName);
        }

        return BuildControlledHybridName(breed1, breed2, pairKey);
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
        AddHybridName(names, "Akita", "Husky", "Frost Warden");
        AddHybridName(names, "Greyhound", "Pit Bull", "Bullhound Hybrid");
        AddHybridName(names, "Wolfdog", "German Shepherd", "Wolf Shepherd Hybrid");
        AddHybridName(names, "Wolfdog", "Husky", "Wolf Husky Hybrid");
        AddHybridName(names, "Alaskan Mastiff", "Rottweiler", "Iron Guard");
        AddHybridName(names, "AlaskanMastiffRottweiler", "Black Inu", "Northjaw Guard");
        AddHybridName(names, "Rott Mastiff Hybrid", "Black Inu", "Northjaw Guard");
        AddHybridName(names, "Pit Bull", "Boxer", "Bull Striker");
        AddHybridName(names, "Greyhound", "Husky", "Frost Runner");

        return names;
    }

    private static void AddHybridName(
        Dictionary<string, string> names,
        string breed1,
        string breed2,
        string hybridName)
    {
        names[MakePairKey(breed1, breed2)] = FinalizeHybridBreedName(hybridName);
    }

    private static Dictionary<BreedFamily, string[]> BuildSameFamilyNames()
    {
        return new Dictionary<BreedFamily, string[]>
        {
            {
                BreedFamily.BullyStriker,
                new[]
                {
                    "Bull Striker",
                    "Apex Bull",
                    "Bully Striker",
                    "Ironjaw"
                }
            },
            {
                BreedFamily.IronRott,
                new[]
                {
                    "Iron Rott",
                    "Blackguard",
                    "Steel Rott",
                    "Obsidian Guard"
                }
            },
            {
                BreedFamily.GuardMastiff,
                new[]
                {
                    "Stone Guard",
                    "War Mastiff",
                    "Guard Prime",
                    "Apex Mastiff"
                }
            },
            {
                BreedFamily.ShepherdSentinel,
                new[]
                {
                    "Shepherd Sentinel",
                    "Scout Sentinel",
                    "Ghost Shepherd",
                    "Ranger Line"
                }
            },
            {
                BreedFamily.SpitzWarden,
                new[]
                {
                    "Frost Warden",
                    "Northwolf",
                    "Spitz Warden",
                    "Whitefang"
                }
            },
            {
                BreedFamily.VelocityHound,
                new[]
                {
                    "Swift Hound",
                    "Ghost Runner",
                    "Velocity Hound",
                    "Runner Prime"
                }
            }
        };
    }

    private static Dictionary<string, string[]> BuildFamilyPairNames()
    {
        Dictionary<string, string[]> names = new Dictionary<string, string[]>();

        AddFamilyPairNames(names, BreedFamily.BullyStriker, BreedFamily.IronRott,
            "Iron Bull", "Blackjaw Bull", "Steel Striker", "Grim Bull");

        AddFamilyPairNames(names, BreedFamily.BullyStriker, BreedFamily.GuardMastiff,
            "Bullguard", "War Bull", "Stone Bull", "Apex Guard");

        AddFamilyPairNames(names, BreedFamily.BullyStriker, BreedFamily.ShepherdSentinel,
            "Bull Sentinel", "Strike Sentinel", "Apex Shepherd", "Guard Striker");

        AddFamilyPairNames(names, BreedFamily.BullyStriker, BreedFamily.SpitzWarden,
            "Bull Warden", "Frost Bull", "North Striker", "Wolf Bull");

        AddFamilyPairNames(names, BreedFamily.BullyStriker, BreedFamily.VelocityHound,
            "Strike Hound", "Bull Runner", "Apex Runner", "Jaw Hound");

        AddFamilyPairNames(names, BreedFamily.IronRott, BreedFamily.GuardMastiff,
            "Iron Guard", "Blackjaw Mastiff", "Stone Rott", "Warjaw Rott",
            "Obsidian Guard", "Cinder Mastiff", "Fortress Rott", "Grimjaw Guard");

        AddFamilyPairNames(names, BreedFamily.IronRott, BreedFamily.ShepherdSentinel,
            "Iron Sentinel", "Blackwatch Rott", "Steel Shepherd", "Grim Sentinel",
            "Ranger Rott", "Obsidian Sentinel");

        AddFamilyPairNames(names, BreedFamily.IronRott, BreedFamily.SpitzWarden,
            "Iron Warden", "Blackfang Warden", "Northjaw Guard", "Obsidian Warden",
            "Cinder Inu", "Ironwolf", "Frostguard Rott");

        AddFamilyPairNames(names, BreedFamily.IronRott, BreedFamily.VelocityHound,
            "Iron Runner", "Black Hound", "Ghost Rott", "Steel Runner");

        AddFamilyPairNames(names, BreedFamily.GuardMastiff, BreedFamily.ShepherdSentinel,
            "Guard Sentinel", "Stone Sentinel", "War Shepherd", "Shield Mastiff",
            "Garrison Sentinel");

        AddFamilyPairNames(names, BreedFamily.GuardMastiff, BreedFamily.SpitzWarden,
            "Frost Guard", "North Guard", "Wolf Mastiff", "Stone Warden",
            "War Warden", "Timber Guard");

        AddFamilyPairNames(names, BreedFamily.GuardMastiff, BreedFamily.VelocityHound,
            "War Runner", "Stone Runner", "Titan Hound", "Guard Runner",
            "Fortress Hound");

        AddFamilyPairNames(names, BreedFamily.ShepherdSentinel, BreedFamily.SpitzWarden,
            "Frost Sentinel", "North Sentinel", "Timber Shepherd", "Wolf Sentinel",
            "Snow Shepherd");

        AddFamilyPairNames(names, BreedFamily.ShepherdSentinel, BreedFamily.VelocityHound,
            "Scout Runner", "Ghost Sentinel", "Ranger Hound", "Swift Shepherd");

        AddFamilyPairNames(names, BreedFamily.SpitzWarden, BreedFamily.VelocityHound,
            "Frost Runner", "Ghost Hound", "North Runner", "Snowdash",
            "Timber Hound", "Whitefang Runner");

        return names;
    }

    private static void AddFamilyPairNames(
        Dictionary<string, string[]> names,
        BreedFamily family1,
        BreedFamily family2,
        params string[] breedNames)
    {
        names[MakeFamilyPairKey(family1, family2)] = breedNames;
    }

    private static string BuildControlledHybridName(string breed1, string breed2, string seed)
    {
        List<BreedFamily> parent1Families = DetectBreedFamilies(breed1);
        List<BreedFamily> parent2Families = DetectBreedFamilies(breed2);
        List<BreedFamily> combinedFamilies = MergeFamilies(parent1Families, parent2Families);

        if (combinedFamilies.Count == 0)
        {
            return PickSafeFallbackName(seed);
        }

        bool deepHybrid = LooksHybridLikeBreedName(breed1) ||
                          LooksHybridLikeBreedName(breed2) ||
                          parent1Families.Count + parent2Families.Count > 2;

        if (combinedFamilies.Count >= 3)
        {
            return FinalizeHybridBreedName(BuildDeepHybridName(combinedFamilies, seed));
        }

        if (combinedFamilies.Count == 1)
        {
            BreedFamily family = combinedFamilies[0];

            if (sameFamilyNames.TryGetValue(family, out string[] names))
            {
                return FinalizeHybridBreedName(PickDeterministicName(names, seed));
            }

            return PickSafeFallbackName(seed);
        }

        string familyPairKey = MakeFamilyPairKey(combinedFamilies[0], combinedFamilies[1]);

        if (familyPairNames.TryGetValue(familyPairKey, out string[] pairNames))
        {
            string breedName = PickDeterministicName(pairNames, seed);

            if (deepHybrid && GetBreedWords(breedName).Length < 3)
            {
                breedName = MaybeAddDeepHybridSuffix(breedName, seed);
            }

            return FinalizeHybridBreedName(breedName);
        }

        return PickSafeFallbackName(seed);
    }

    private static string BuildDeepHybridName(List<BreedFamily> families, string seed)
    {
        if (ContainsFamily(families, BreedFamily.IronRott) &&
            ContainsFamily(families, BreedFamily.GuardMastiff) &&
            ContainsFamily(families, BreedFamily.SpitzWarden))
        {
            string[] ironGuardSpitzNames =
            {
                "Northjaw Guard",
                "Blackfang Warden",
                "Obsidian Warden"
            };

            return PickDeterministicName(ironGuardSpitzNames, seed);
        }

        if (ContainsFamily(families, BreedFamily.BullyStriker) &&
            ContainsFamily(families, BreedFamily.ShepherdSentinel) &&
            ContainsFamily(families, BreedFamily.SpitzWarden))
        {
            string[] names =
            {
                "Apex Wolf Sentinel",
                "Bull Warden Prime",
                "Cinder Shepherd Legacy"
            };

            return PickDeterministicName(names, seed);
        }

        return PickDeterministicName(deepHybridNames, seed);
    }

    private static string MaybeAddDeepHybridSuffix(string breedName, string seed)
    {
        string[] suffixes =
        {
            "Line",
            "Strain",
            "Legacy",
            "Prime",
            "Variant",
            "Blood"
        };

        string suffix = PickDeterministicName(suffixes, $"{seed}|suffix");
        return $"{breedName} {suffix}";
    }

    private static string PickSafeFallbackName(string seed)
    {
        return FinalizeHybridBreedName(PickDeterministicName(safeFallbackNames, seed));
    }

    private static List<BreedFamily> DetectBreedFamilies(string breedName)
    {
        List<BreedFamily> families = new List<BreedFamily>();
        string rawBreed = GetRawBreedText(breedName);
        string separatedBreed = GetSeparatedBreedText(breedName);
        string compactBreed = GetCompactBreedText(breedName);

        AddFamilyIfMatches(families, BreedFamily.BullyStriker,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "pit bull", "pitbull", "american bully", "bully",
                "staffordshire", "boxer", "bulldog", "bull", "striker", "strike"));

        AddFamilyIfMatches(families, BreedFamily.IronRott,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "rottweiler", "rott", "doberman", "beauceron",
                "black russian terrier", "blackrussianterrier",
                "iron", "black", "obsidian", "steel", "cinder"));

        AddFamilyIfMatches(families, BreedFamily.GuardMastiff,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "mastiff", "cane corso", "canecorso", "presa", "dogo",
                "boerboel", "english mastiff", "tibetan mastiff",
                "great dane", "greatdane", "kangal", "tosa", "fila",
                "central asian shepherd", "centralasianshepherd",
                "alaskan mastiff", "alaskanmastiff",
                "guard", "war", "stone", "fortress", "garrison", "shield"));

        AddFamilyIfMatches(families, BreedFamily.ShepherdSentinel,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "german shepherd", "germanshepherd", "german shepard", "germanshepard",
                "belgian malinois", "belgianmalinois", "dutch shepherd",
                "dutchshepherd", "anatolian shepherd", "anatolianshepherd",
                "australian shepherd", "australianshepherd",
                "shepherd", "shepard", "malinois", "sentinel", "ranger", "scout"));

        AddFamilyIfMatches(families, BreedFamily.SpitzWarden,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "akita", "husky", "alaskan malamute", "alaskanmalamute",
                "malamute", "shiba inu", "shibainu", "black inu", "blackinu",
                "inu", "chow chow", "chowchow", "samoyed", "spitz",
                "warden", "north", "frost", "wolf", "timber", "snow", "whitefang"));

        AddFamilyIfMatches(families, BreedFamily.VelocityHound,
            ContainsAny(rawBreed, separatedBreed, compactBreed,
                "greyhound", "whippet", "saluki", "rhodesian ridgeback",
                "rhodesianridgeback", "pharaoh hound", "pharaohhound",
                "thai ridgeback", "thairidgeback", "catahoula leopard dog",
                "catahoulaleoparddog", "catahoula", "hound", "runner", "swift", "dash"));

        return OrderFamilies(families);
    }

    private static void AddFamilyIfMatches(List<BreedFamily> families, BreedFamily family, bool matches)
    {
        if (matches && !families.Contains(family))
        {
            families.Add(family);
        }
    }

    private static List<BreedFamily> MergeFamilies(List<BreedFamily> familySet1, List<BreedFamily> familySet2)
    {
        List<BreedFamily> merged = new List<BreedFamily>();

        AddFamilies(merged, familySet1);
        AddFamilies(merged, familySet2);

        return OrderFamilies(merged);
    }

    private static void AddFamilies(List<BreedFamily> target, List<BreedFamily> source)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            BreedFamily family = source[i];

            if (family != BreedFamily.Unknown && !target.Contains(family))
            {
                target.Add(family);
            }
        }
    }

    private static List<BreedFamily> OrderFamilies(List<BreedFamily> families)
    {
        List<BreedFamily> ordered = new List<BreedFamily>();

        for (int i = 0; i < familyOrder.Length; i++)
        {
            BreedFamily family = familyOrder[i];

            if (families != null && families.Contains(family))
            {
                ordered.Add(family);
            }
        }

        return ordered;
    }

    private static bool ContainsFamily(List<BreedFamily> families, BreedFamily family)
    {
        return families != null && families.Contains(family);
    }

    private static string MakeFamilyPairKey(BreedFamily family1, BreedFamily family2)
    {
        if ((int)family1 <= (int)family2)
        {
            return $"{family1}|{family2}";
        }

        return $"{family2}|{family1}";
    }

    private static bool ContainsAny(
        string rawBreed,
        string separatedBreed,
        string compactBreed,
        params string[] searchTexts)
    {
        if (searchTexts == null)
        {
            return false;
        }

        for (int i = 0; i < searchTexts.Length; i++)
        {
            string searchText = searchTexts[i];

            if (string.IsNullOrWhiteSpace(searchText))
            {
                continue;
            }

            string rawSearch = searchText.Trim().ToLowerInvariant();
            string separatedSearch = GetSeparatedBreedText(searchText);
            string compactSearch = GetCompactBreedText(searchText);

            if ((!string.IsNullOrEmpty(rawSearch) && rawBreed.Contains(rawSearch)) ||
                (!string.IsNullOrEmpty(separatedSearch) && separatedBreed.Contains(separatedSearch)) ||
                (!string.IsNullOrEmpty(compactSearch) && compactBreed.Contains(compactSearch)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LooksHybridLikeBreedName(string breedName)
    {
        string rawBreed = GetRawBreedText(breedName);
        string separatedBreed = GetSeparatedBreedText(breedName);
        string compactBreed = GetCompactBreedText(breedName);

        if (ContainsAny(rawBreed, separatedBreed, compactBreed,
            "guard", "warden", "sentinel", "runner", "striker",
            "line", "strain", "legacy", "prime", "hybrid", "variant", "blood"))
        {
            return true;
        }

        return DetectBreedFamilies(breedName).Count > 1;
    }

    private static string FinalizeHybridBreedName(string breedName)
    {
        string cleanedName = CollapseSpaces(ExpandKnownCompactBreedName(CleanBreedName(breedName)));

        if (LooksLikeRawBreedConcatenation(cleanedName))
        {
            return "Apex Hybrid";
        }

        string[] parts = GetBreedWords(cleanedName);

        if (parts.Length == 0)
        {
            return "Mixed Line";
        }

        if (parts.Length <= 3)
        {
            return string.Join(" ", parts);
        }

        return $"{parts[0]} {parts[1]} {parts[2]}";
    }

    private static string ExpandKnownCompactBreedName(string breedName)
    {
        string compactBreed = GetCompactBreedText(breedName);

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

            case "alaskanmastiffrottweiler":
                return "Alaskan Mastiff Rottweiler";

            default:
                return breedName;
        }
    }

    private static bool LooksLikeRawBreedConcatenation(string breedName)
    {
        string compactBreed = GetCompactBreedText(breedName);

        if (compactBreed.Length < 18 || GetBreedWords(breedName).Length > 1)
        {
            return false;
        }

        return DetectBreedFamilies(breedName).Count > 1;
    }

    private static string PickDeterministicName(string[] names, string seed)
    {
        if (names == null || names.Length == 0)
        {
            return "Mixed Line";
        }

        int index = GetDeterministicIndex(seed, names.Length);
        return names[index];
    }

    private static int GetDeterministicIndex(string seed, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        uint hash = 2166136261;
        string safeSeed = seed ?? string.Empty;

        for (int i = 0; i < safeSeed.Length; i++)
        {
            hash ^= char.ToLowerInvariant(safeSeed[i]);
            hash *= 16777619;
        }

        return (int)(hash % count);
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
