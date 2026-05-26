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

    [Header("Buttons")]
    public Button selectAsFighter1Button;
    public Button selectAsFighter2Button;

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
            string status = GetStatusText();

            statsText.text =
                $"Breed: {dog.breed}\n" +
                $"Status: {status} | Age: {dog.age} | {dog.GetLifeStage()}\n" +
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

        if (selectAsFighter1Button != null)
        {
            selectAsFighter1Button.onClick.RemoveAllListeners();
            selectAsFighter1Button.onClick.AddListener(SelectAsFighter1);
            selectAsFighter1Button.interactable = CanSelectDog();
        }

        if (selectAsFighter2Button != null)
        {
            selectAsFighter2Button.onClick.RemoveAllListeners();
            selectAsFighter2Button.onClick.AddListener(SelectAsFighter2);
            selectAsFighter2Button.interactable = CanSelectDog();
        }
    }

    string GetStatusText()
    {
        if (dog.isDead)
        {
            return "Deceased";
        }

        if (dog.isRetired)
        {
            return "Retired";
        }

        return "Active";
    }

    bool CanSelectDog()
    {
        if (dog == null) return false;
        if (dog.isDead) return false;
        if (dog.isRetired) return false;

        return true;
    }

    void SelectAsFighter1()
    {
        if (!CanSelectDog())
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected.");
            return;
        }

        if (dogManager != null)
        {
            dogManager.SelectFighter1(dog);
        }
        else
        {
            Debug.LogError("DogManager reference is missing on DogCardUI.");
        }
    }

    void SelectAsFighter2()
    {
        if (!CanSelectDog())
        {
            Debug.LogWarning($"{dog.dogName} cannot be selected.");
            return;
        }

        if (dogManager != null)
        {
            dogManager.SelectFighter2(dog);
        }
        else
        {
            Debug.LogError("DogManager reference is missing on DogCardUI.");
        }
    }
}
