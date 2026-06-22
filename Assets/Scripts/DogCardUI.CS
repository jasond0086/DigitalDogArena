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
                $"{dog.breed} | {dog.gender}\n" +
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
}
