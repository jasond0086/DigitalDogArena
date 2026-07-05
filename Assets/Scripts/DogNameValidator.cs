using System;
using System.Collections.Generic;
using System.Text;

public static class DogNameValidator
{
    public const string RealDogNameMessage = "Use a real dog name.";
    public const string NotAllowedMessage = "That name is not allowed.";

    private const int MinNameLength = 2;
    private const int MaxNameLength = 18;
    private const int MaxSpaceSeparatedWords = 2;

    private static readonly HashSet<string> approvedWords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ace", "atlas", "bandit", "bear", "blaze", "blitz", "blue", "bolt", "boss", "bronx", "brute", "buck", "bullet",
            "cash", "champ", "chief", "copper", "diesel", "duke", "fang", "ghost", "king", "knox", "luna", "nova",
            "nyx", "onyx", "ranger", "rebel", "rex", "rocco", "rogue", "sable", "shadow", "spike", "storm", "titan",
            "vega", "viper", "wolf", "zeus", "kira", "juno", "rocky", "max", "bella", "nala", "thor", "apollo",
            "kane", "roxy", "ruby", "scout", "maverick", "loki", "odin", "hera", "athena", "xena",
            "jax", "axel", "gage", "drake", "nero", "roman", "kilo", "echo", "raven", "jade", "pearl", "ash",
            "iron", "steel", "ember", "smoke", "frost", "crimson", "scarlet", "midnight", "thunder",
            "hunter", "guardian", "sentinel", "apex", "alpha", "savage", "noble", "valor", "havoc",
            "phantom", "spirit", "venom", "chaos", "riot", "comet", "solar", "lunar",
            "obsidian", "silver", "golden", "red", "black", "white", "gray",
            "neo", "vex", "kairo", "zoro"
        };

    private static readonly HashSet<string> generatedNamePrefixes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "neo", "vex", "rex", "kairo", "zoro", "luna", "nyx", "titan", "ash", "onyx", "rogue", "fang"
        };

    private static readonly HashSet<string> generatedNameSuffixes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fang", "claw", "storm", "shade", "strike", "maw", "blood", "ghost", "bane", "howl", "rift", "volt"
        };

    private static readonly HashSet<string> blockedTerms =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fuck", "shit", "bitch", "cunt", "asshole", "whore", "slut",
            "nigger", "nigga", "faggot", "fag", "kike", "spic", "chink", "gook", "retard", "dyke",
            "nazi", "hitler", "rape", "rapist", "kill", "murder", "suicide", "selfharm"
        };

    public static bool TryValidateName(string requestedName, out string cleanedName, out string message)
    {
        cleanedName = CleanSpacing(requestedName);
        message = RealDogNameMessage;

        if (string.IsNullOrEmpty(cleanedName) ||
            cleanedName.Length < MinNameLength ||
            cleanedName.Length > MaxNameLength)
        {
            return false;
        }

        if (ContainsBlockedTerm(cleanedName))
        {
            message = NotAllowedMessage;
            return false;
        }

        if (!HasOnlyAllowedCharacters(cleanedName) ||
            HasBadSeparatorUse(cleanedName) ||
            CountSpaceSeparatedWords(cleanedName) > MaxSpaceSeparatedWords)
        {
            return false;
        }

        if (!AllTokensAreApproved(cleanedName))
        {
            return false;
        }

        cleanedName = ToDisplayCase(cleanedName);
        message = string.Empty;
        return true;
    }

    public static bool IsSameVisibleName(string requestedName, string currentName)
    {
        string requestedClean = CleanSpacing(requestedName);
        string currentClean = CleanSpacing(currentName);

        return string.Equals(requestedClean, currentClean, StringComparison.Ordinal);
    }

    private static string CleanSpacing(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder(value.Length);
        bool previousWasSpace = false;

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];

            if (char.IsWhiteSpace(current))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }

                continue;
            }

            builder.Append(current);
            previousWasSpace = false;
        }

        return builder.ToString().Trim();
    }

    private static bool HasOnlyAllowedCharacters(string value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];

            if (char.IsLetter(current) ||
                current == ' ' ||
                current == '-' ||
                current == '\'')
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool HasBadSeparatorUse(string value)
    {
        if (value.Length == 0)
        {
            return true;
        }

        if (IsNameSeparator(value[0]) || IsNameSeparator(value[value.Length - 1]))
        {
            return true;
        }

        for (int i = 1; i < value.Length; i++)
        {
            if (IsNameSeparator(value[i]) && IsNameSeparator(value[i - 1]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNameSeparator(char value)
    {
        return value == ' ' || value == '-' || value == '\'';
    }

    private static int CountSpaceSeparatedWords(string value)
    {
        return value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static bool ContainsBlockedTerm(string value)
    {
        string normalized = NormalizeForBlocklist(value);

        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        foreach (string blockedTerm in blockedTerms)
        {
            if (normalized.Contains(blockedTerm))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeForBlocklist(string value)
    {
        StringBuilder builder = new StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            char current = char.ToLowerInvariant(value[i]);
            char mapped = MapLeetCharacter(current);

            if (char.IsLetter(mapped))
            {
                builder.Append(mapped);
            }
        }

        return CollapseRepeatedLetters(builder.ToString());
    }

    private static char MapLeetCharacter(char value)
    {
        switch (value)
        {
            case '0': return 'o';
            case '1': return 'i';
            case '3': return 'e';
            case '4': return 'a';
            case '5': return 's';
            case '7': return 't';
            case '@': return 'a';
            case '$': return 's';
            case '!': return 'i';
            default: return value;
        }
    }

    private static string CollapseRepeatedLetters(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder(value.Length);
        char previous = '\0';

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];

            if (current == previous)
            {
                continue;
            }

            builder.Append(current);
            previous = current;
        }

        return builder.ToString();
    }

    private static bool AllTokensAreApproved(string value)
    {
        string[] tokens = value.Split(
            new[] { ' ', '-', '\'' },
            StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i].ToLowerInvariant();

            if (approvedWords.Contains(token) || IsApprovedGeneratedCompound(token))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsApprovedGeneratedCompound(string token)
    {
        foreach (string prefix in generatedNamePrefixes)
        {
            if (!token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string suffix = token.Substring(prefix.Length);

            if (suffix.Length > 0 && generatedNameSuffixes.Contains(suffix))
            {
                return true;
            }
        }

        return false;
    }

    private static string ToDisplayCase(string value)
    {
        StringBuilder builder = new StringBuilder(value.Length);
        bool capitalizeNext = true;

        for (int i = 0; i < value.Length; i++)
        {
            char current = value[i];

            if (char.IsLetter(current))
            {
                builder.Append(capitalizeNext
                    ? char.ToUpperInvariant(current)
                    : char.ToLowerInvariant(current));
                capitalizeNext = false;
                continue;
            }

            builder.Append(current);
            capitalizeNext = current == ' ' || current == '-' || current == '\'';
        }

        return builder.ToString();
    }
}
