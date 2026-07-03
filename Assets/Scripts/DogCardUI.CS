using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DogCardUI : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI strengthValueText;
    public TextMeshProUGUI agilityValueText;
    public TextMeshProUGUI staminaValueText;
    public TextMeshProUGUI winsLossesValueText;
    public TextMeshProUGUI levelValueText;
    public TextMeshProUGUI bloodlineText;

    [Header("Optional Image")]
    public Image dogImage;
    public Sprite defaultDogSprite;

    private Dog dog;
    private DogManager dogManager;

    public void Setup(Dog dogToDisplay, DogManager manager)
    {
        dog = dogToDisplay;
        dogManager = manager;

        if (dog == null)
        {
            Debug.LogError("DogCardUI received a null dog.");
            return;
        }

        if (nameText != null)
        {
            nameText.text = dog.dogName;

        }
        if (statsText != null)
        {
            string breedingStatus = GetBreedingStatusText();

            statsText.text =
                $"Breed: {GetDisplayBreedName(dog.breed)}\n" +
                $"Gender: {dog.gender}\n" +
                $"Age {dog.age} | {dog.GetLifeStage()}\n" +
                $"{dog.GetStatusText()} | {breedingStatus}\n" +
                $"{dog.GetPotentialTier()} | {dog.GetPotentialTitle()}\n" +
                $"Style: {dog.fightStyle}\n" +
                $"Traits: {dog.GetTraitSummary()}";
        }

        SetText(strengthValueText, $"STR {dog.strength}/{dog.strengthPotential}");
        SetText(agilityValueText, $"AGI {dog.agility}/{dog.agilityPotential}");
        SetText(staminaValueText, $"STA {dog.stamina}/{dog.staminaPotential}");
        SetText(winsLossesValueText, $"Record {dog.wins}W - {dog.losses}L");
        SetText(levelValueText, $"Level {dog.level}");
        SetText(bloodlineText, GetBloodlineText());

        if (dogImage != null)
        {
            dogImage.sprite = dog.dogSprite != null ? dog.dogSprite : defaultDogSprite;
            dogImage.color = Color.white;
            dogImage.enabled = dogImage.sprite != null;
        }
    }

    string GetBreedingStatusText()
    {
        if (dog == null || dogManager == null)
        {
            return "Last Bred Week: Unknown";
        }

        if (dog.CanBreedInWeek(dogManager.currentWeek))
        {
            return "Can Breed This Week";
        }

        return $"Already Bred This Week | Last Bred Week: {dog.lastBredWeek}";
    }

    void SetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    string GetBloodlineText()
    {
        bool hasBloodline = !string.IsNullOrWhiteSpace(dog.bloodlineName);
        bool hasAncestorSummary = !string.IsNullOrWhiteSpace(dog.ancestorBonusSummary);

        if (!hasBloodline && !dog.isBloodlineCarrier)
        {
            return "Bloodline: None";
        }

        string bloodlineName = hasBloodline ? dog.bloodlineName : "Unknown";
        string text = $"Bloodline: {bloodlineName}";

        if (hasAncestorSummary)
        {
            text += $"\nAncestor: {dog.ancestorBonusSummary}";
        }

        if (dog.isBloodlineCarrier)
        {
            text += "\nCarrier";
        }

        return text;
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
}
