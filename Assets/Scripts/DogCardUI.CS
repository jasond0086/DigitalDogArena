using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DogCardUI : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;

    [Header("Optional Image")]
    public Image dogImage;

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
                $"Breed: {dog.breed}\n" +
                $"Gender: {dog.gender} | {breedingStatus}\n" +
                $"Status: {dog.GetStatusText()} | Age: {dog.age} | Life Stage: {dog.GetLifeStage()}\n" +
                $"STR {dog.strength}/{dog.strengthPotential} | " +
                $"AGI {dog.agility}/{dog.agilityPotential} | " +
                $"STA {dog.stamina}/{dog.staminaPotential}\n" +
                $"{dog.GetPotentialTier()} | {dog.GetPotentialTitle()}\n" +
                $"Style: {dog.fightStyle} | Traits: {dog.GetTraitSummary()}\n" +
                $"Growth: x{dog.growthRate:F2}\n" +
                $"Gen {dog.generation} | Lvl {dog.level} | XP {dog.xp}/{dog.xpToNextLevel}\n" +
                $"Record: {dog.wins}-{dog.losses}";
        }

        if (dogImage != null && dog.dogSprite != null)
        {
            dogImage.sprite = dog.dogSprite;
            dogImage.enabled = true;
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
}
